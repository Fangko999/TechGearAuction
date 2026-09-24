using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Appeals.Commands;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Domain.Exceptions;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Appeals;

public class ResolveAppealTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public ResolveAppealTests()
    {
        _factory = new TestDbFactory();
    }

    [Fact]
    public async Task ResolveAppeal_Approved_ShouldUnbanUserAndRemoveDeviceFromBlacklist()
    {
        var appealId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            var user = await setupCtx.Users.FindAsync(TestDbFactory.User2Id);
            user!.Status = UserStatus.Banned;
            user.LastLoginDeviceHash = "banned-hash";
            
            setupCtx.BannedDevices.Add(new BannedDevice { DeviceHash = "banned-hash", Reason = "spam", BannedAt = DateTime.UtcNow });
            
            setupCtx.Appeals.Add(new Appeal { Id = appealId, UserId = TestDbFactory.User2Id, Description = "Sorry", Status = AppealStatus.Pending, CreatedAt = DateTime.UtcNow });
            
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var ctx = _factory.CreateContext();
        var handler = new ResolveAppealCommandHandler(ctx, svc);

        await handler.Handle(new ResolveAppealCommand { AppealId = appealId, Approve = true, AdminNote = "Ok" }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var userAfter = await verifyCtx.Users.FindAsync(TestDbFactory.User2Id);
        userAfter!.Status.Should().Be(UserStatus.Active);

        var device = await verifyCtx.BannedDevices.FirstOrDefaultAsync(b => b.DeviceHash == "banned-hash");
        device.Should().BeNull();

        var appeal = await verifyCtx.Appeals.FindAsync(appealId);
        appeal!.Status.Should().Be(AppealStatus.Approved);
        
        var log = await verifyCtx.AdminAuditLogs.FirstOrDefaultAsync(l => l.EntityId == appealId);
        log.Should().NotBeNull();
        log!.Details.Should().Contain("approved");
    }

    [Fact]
    public async Task ResolveAppeal_Rejected_ShouldKeepBannedStatus()
    {
        var appealId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            var user = await setupCtx.Users.FindAsync(TestDbFactory.User2Id);
            user!.Status = UserStatus.Banned;
            
            setupCtx.Appeals.Add(new Appeal { Id = appealId, UserId = TestDbFactory.User2Id, Description = "Sorry", Status = AppealStatus.Pending, CreatedAt = DateTime.UtcNow });
            
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var ctx = _factory.CreateContext();
        var handler = new ResolveAppealCommandHandler(ctx, svc);

        await handler.Handle(new ResolveAppealCommand { AppealId = appealId, Approve = false, AdminNote = "No" }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var userAfter = await verifyCtx.Users.FindAsync(TestDbFactory.User2Id);
        userAfter!.Status.Should().Be(UserStatus.Banned);

        var appeal = await verifyCtx.Appeals.FindAsync(appealId);
        appeal!.Status.Should().Be(AppealStatus.Rejected);
    }
    
    [Fact]
    public async Task ResolveAppeal_NotFound_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var ctx = _factory.CreateContext();
        var handler = new ResolveAppealCommandHandler(ctx, svc);

        var act = () => handler.Handle(new ResolveAppealCommand { AppealId = Guid.NewGuid(), Approve = true, AdminNote = "No" }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose() => _factory.Dispose();
}
