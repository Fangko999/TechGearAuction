using FluentAssertions;
using TechGearAuction.Application.Features.Admin.Queries;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Admin;

public class AdminAuditLogTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public AdminAuditLogTests()
    {
        _factory = new TestDbFactory();
    }

    [Fact]
    public async Task GetAuditLogs_ShouldReturnPaginatedOrderByDateDesc()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.AdminAuditLogs.AddRange(
                new AdminAuditLog { Id = Guid.NewGuid(), AdminId = TestDbFactory.AdminId, Action = "A1", EntityType = "T", EntityId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                new AdminAuditLog { Id = Guid.NewGuid(), AdminId = TestDbFactory.AdminId, Action = "A2", EntityType = "T", EntityId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddMinutes(-1) }
            );
            await setupCtx.SaveChangesAsync();
        }

        var ctx = _factory.CreateContext();
        var handler = new GetAuditLogsQueryHandler(ctx);

        var result = await handler.Handle(new GetAuditLogsQuery { PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        result.Items.Count.Should().Be(2);
        result.Items.First().Action.Should().Be("A2"); // Newest first
    }
    
    [Fact]
    public async Task GetAuditLogs_ShouldFilterByAdminId()
    {
        var otherAdminId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Users.Add(new User { Id = otherAdminId, Email = "admin2@test.com", DisplayName = "Admin2", PasswordHash = "123", Role = TechGearAuction.Domain.Enums.UserRole.Admin });
            setupCtx.AdminAuditLogs.AddRange(
                new AdminAuditLog { Id = Guid.NewGuid(), AdminId = TestDbFactory.AdminId, Action = "A1", EntityType = "T", EntityId = Guid.NewGuid() },
                new AdminAuditLog { Id = Guid.NewGuid(), AdminId = otherAdminId, Action = "A2", EntityType = "T", EntityId = Guid.NewGuid() }
            );
            await setupCtx.SaveChangesAsync();
        }

        var ctx = _factory.CreateContext();
        var handler = new GetAuditLogsQueryHandler(ctx);

        var result = await handler.Handle(new GetAuditLogsQuery { AdminId = otherAdminId }, CancellationToken.None);

        result.Items.Count.Should().Be(1);
        result.Items.First().AdminId.Should().Be(otherAdminId);
    }

    public void Dispose() => _factory.Dispose();
}
