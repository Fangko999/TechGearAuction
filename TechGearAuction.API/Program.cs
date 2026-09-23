using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Infrastructure.Data;
using TechGearAuction.Infrastructure.Services;
using TechGearAuction.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
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
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<TechGearAuction.API.Middlewares.DeviceBlacklistMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TechGearAuction.API.Hubs.AuctionHub>("/hubs/auction");
app.MapHub<TechGearAuction.API.Hubs.ChatHub>("/hubs/chat");

using (var scope = app.Services.CreateScope())
{
    var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
    try
    {
        await storageService.InitializeBucketsAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing Minio buckets.");
    }
}

app.Run();