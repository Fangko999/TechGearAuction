using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Infrastructure.Data;

namespace TechGearAuction.Tests.Helpers;

/// <summary>
/// Factory tạo AppDbContext dùng SQLite InMemory cho mỗi test case.
/// Mỗi test nên gọi CreateContext() để nhận một DB sạch.
/// </summary>
public class TestDbFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    // Seed data IDs — cố định để test case có thể tham chiếu
    public static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid UserId  = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid User2Id = Guid.Parse("00000000-0000-0000-0000-000000000003");

    public TestDbFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Tạo schema
        using var ctx = new AppDbContext(_options);
        ctx.Database.EnsureCreated();

        SeedData(ctx);
    }

    public AppDbContext CreateContext() => new AppDbContext(_options);

    private static void SeedData(AppDbContext ctx)
    {
        var adminUser = new User
        {
            Id = AdminId,
            Email = "admin@test.com",
            DisplayName = "Admin User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            AvailableCredits = 10,
            CreatedAt = DateTime.UtcNow
        };

        var normalUser = new User
        {
            Id = UserId,
            Email = "user@test.com",
            DisplayName = "Normal User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            Role = UserRole.User,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            AvailableCredits = 5,
            CreatedAt = DateTime.UtcNow
        };

        var user2 = new User
        {
            Id = User2Id,
            Email = "user2@test.com",
            DisplayName = "Second User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            Role = UserRole.User,
            Status = UserStatus.Active,
            IsEmailVerified = true,
            AvailableCredits = 3,
            CreatedAt = DateTime.UtcNow
        };

        ctx.Users.AddRange(adminUser, normalUser, user2);

        // Seed a credit transaction for normalUser
        ctx.CreditTransactions.Add(new CreditTransaction
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Amount = 3,
            Reason = "Signup Bonus",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });

        ctx.SaveChanges();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}

