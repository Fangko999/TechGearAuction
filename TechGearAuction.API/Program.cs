using Microsoft.EntityFrameworkCore;
using TechGearAuction.Infrastructure.Data; // Nhớ using namespace này

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- THÊM ĐOẠN NÀY ĐỂ CẤU HÌNH EF CORE ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("TechGearAuction.Infrastructure")));
// Lưu ý dòng MigrationsAssembly: Báo cho EF Core biết thư mục chứa Migrations nằm ở project Infrastructure

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();