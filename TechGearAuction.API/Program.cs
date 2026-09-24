using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Infrastructure.Data;
using TechGearAuction.Infrastructure.Services;
using TechGearAuction.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to use JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TechGearAuction API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

// Configure EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("TechGearAuction.Infrastructure")));

// Configure Dependency Injection
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, MockEmailService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

// Configure MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TechGearAuction.Application.Features.Users.Commands.UpdateMyProfileCommand).Assembly));

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("MinioSettings"));
builder.Services.AddScoped<IStorageService, MinioStorageService>();
builder.Services.AddScoped<IAuctionNotificationService, AuctionNotificationService>();
builder.Services.AddScoped<IChatNotificationService, ChatNotificationService>();

builder.Services.AddHostedService<AuctionClosingBackgroundService>();
builder.Services.AddHostedService<ChatRoomArchivingBackgroundService>();

builder.Services.AddSignalR();

// Add CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "https://techgearauction.vn" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.WithOrigins(allowedOrigins)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<TechGearAuction.Application.Interfaces.ITokenBlacklistService, TechGearAuction.Infrastructure.Services.TokenBlacklistService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(5);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var blacklistService = context.HttpContext.RequestServices.GetRequiredService<TechGearAuction.Application.Interfaces.ITokenBlacklistService>();
                var token = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                if (token != null && blacklistService.IsTokenBlacklisted(token.RawData))
                {
                    context.Fail("Token has been revoked.");
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    await next();
});

app.UseMiddleware<TechGearAuction.API.Middlewares.GlobalExceptionMiddleware>();

app.UseRateLimiter();
app.UseRouting();
app.UseCors("AllowAll");
app.UseWebSockets();

app.UseMiddleware<TechGearAuction.API.Middlewares.DeviceBlacklistMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TechGearAuction.API.Hubs.AuctionHub>("/hubs/auction");
app.MapHub<TechGearAuction.API.Hubs.ChatHub>("/hubs/chat");

using (var scope = app.Services.CreateScope())
{
    var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
    var dbContext = scope.ServiceProvider.GetRequiredService<TechGearAuction.Infrastructure.Data.AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Apply migrations
        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.MigrateAsync(dbContext.Database);
        
        // Seed Data
        await TechGearAuction.Infrastructure.Data.DataSeeder.SeedDataAsync(dbContext);
        
        // Initialize MinIO Buckets
        await storageService.InitializeBucketsAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during startup initialization.");
    }
}

app.Run();

public partial class Program { }