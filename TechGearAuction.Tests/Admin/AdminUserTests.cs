using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Features.Users.Commands;
using TechGearAuction.Application.Features.Users.Queries;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Admin;

public class AdminUserTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public AdminUserTests() => _factory = new TestDbFactory();

    // ─── GetUsers ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_Admin_ShouldReturnPaginatedResult()
    {
        var ctx = _factory.CreateContext();
        var handler = new GetUsersQueryHandler(ctx);

        var result = await handler.Handle(new GetUsersQuery { PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
        result.PageIndex.Should().Be(1);
    }

    [Fact]
    public async Task GetUsers_Admin_ShouldIncludeSoftDeletedUsers()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            var u = await setupCtx.Users.IgnoreQueryFilters().FirstAsync(x => x.Id == TestDbFactory.User2Id);
            u.DeletedAt = DateTime.UtcNow;
            u.Status = UserStatus.Closed;
            await setupCtx.SaveChangesAsync();
        }

        var ctx = _factory.CreateContext();
        var handler = new GetUsersQueryHandler(ctx);
        var result = await handler.Handle(new GetUsersQuery { PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(3, "Admin should see ALL users including soft-deleted ones");
        result.Items.Should().Contain(u => u.Id == TestDbFactory.User2Id);
    }

    // ─── AdminUpdateUserProfile ───────────────────────────────────────────────

    [Fact]
    public async Task AdminUpdateUserProfile_ShouldUpdateDataAndCreateAuditLog()
    {
        var adminSvc = new MockCurrentUserService(TestDbFactory.AdminId, "Admin");
        var ctx = _factory.CreateContext();
        var handler = new UpdateUserByAdminCommandHandler(ctx, adminSvc);

        await handler.Handle(new UpdateUserByAdminCommand
        {
            TargetUserId = TestDbFactory.UserId,
            DisplayName = "Admin-Changed Name",
            SocialLinks = new List<SocialLinkDto>()
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var updated = await verifyCtx.Users.FirstAsync(u => u.Id == TestDbFactory.UserId);
        updated.DisplayName.Should().Be("Admin-Changed Name");

        var auditLog = await verifyCtx.AdminAuditLogs
            .FirstOrDefaultAsync(l => l.AdminId == TestDbFactory.AdminId && l.EntityId == TestDbFactory.UserId);
        auditLog.Should().NotBeNull("an audit log must be recorded for Admin actions");
        auditLog!.Action.Should().Be("Update Profile");
    }

    // ─── CloseAccount ────────────────────────────────────────────────────────

    [Fact]
    public async Task CloseAccount_ShouldSetStatusClosedAndDeletedAt()
    {
        var adminSvc = new MockCurrentUserService(TestDbFactory.AdminId, "Admin");
        var ctx = _factory.CreateContext();
        var handler = new CloseUserAccountCommandHandler(ctx, adminSvc);

        await handler.Handle(new CloseUserAccountCommand { TargetUserId = TestDbFactory.UserId }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var closed = await verifyCtx.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == TestDbFactory.UserId);
        closed.Status.Should().Be(UserStatus.Closed);
        closed.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CloseAccount_OnSelf_ShouldThrow()
    {
        var adminSvc = new MockCurrentUserService(TestDbFactory.AdminId, "Admin");
        var ctx = _factory.CreateContext();
        var handler = new CloseUserAccountCommandHandler(ctx, adminSvc);

        var act = () => handler.Handle(
            new CloseUserAccountCommand { TargetUserId = TestDbFactory.AdminId }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RestoreAccount_ShouldClearDeletedAtAndSetActiveStatus()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            var u = await setupCtx.Users.IgnoreQueryFilters().FirstAsync(x => x.Id == TestDbFactory.UserId);
            u.Status = UserStatus.Closed;
            u.DeletedAt = DateTime.UtcNow;
            await setupCtx.SaveChangesAsync();
        }

        var adminSvc = new MockCurrentUserService(TestDbFactory.AdminId, "Admin");
        var ctx = _factory.CreateContext();
        var handler = new RestoreUserAccountCommandHandler(ctx, adminSvc);

        await handler.Handle(new RestoreUserAccountCommand { TargetUserId = TestDbFactory.UserId }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var restored = await verifyCtx.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == TestDbFactory.UserId);
        restored.Status.Should().Be(UserStatus.Active);
        restored.DeletedAt.Should().BeNull();
    }

    // ─── BanUser ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task BanUser_ShouldSetStatusBannedAndCreateAuditLog()
    {
        var adminSvc = new MockCurrentUserService(TestDbFactory.AdminId, "Admin");
        var ctx = _factory.CreateContext();
        var handler = new BanUserCommandHandler(ctx, adminSvc);

        await handler.Handle(new BanUserCommand
        {
            TargetUserId = TestDbFactory.UserId,
            Reason = "Violation of terms"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var banned = await verifyCtx.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == TestDbFactory.UserId);
        banned.Status.Should().Be(UserStatus.Banned);

        var auditLog = await verifyCtx.AdminAuditLogs
            .FirstOrDefaultAsync(l => l.AdminId == TestDbFactory.AdminId && l.Action == "Ban User");
        auditLog.Should().NotBeNull("audit log must be created on ban action");
    }

    [Fact]
    public async Task BanUser_OnSelf_ShouldThrow()
    {
        var adminSvc = new MockCurrentUserService(TestDbFactory.AdminId, "Admin");
        var ctx = _factory.CreateContext();
        var handler = new BanUserCommandHandler(ctx, adminSvc);

        var act = () => handler.Handle(
            new BanUserCommand { TargetUserId = TestDbFactory.AdminId, Reason = "Test" }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UnbanUser_ShouldSetStatusActive()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            var u = await setupCtx.Users.IgnoreQueryFilters().FirstAsync(x => x.Id == TestDbFactory.UserId);
            u.Status = UserStatus.Banned;
            await setupCtx.SaveChangesAsync();
        }

        var adminSvc = new MockCurrentUserService(TestDbFactory.AdminId, "Admin");
        var ctx = _factory.CreateContext();
        var handler = new UnbanUserCommandHandler(ctx, adminSvc);

        await handler.Handle(new UnbanUserCommand { TargetUserId = TestDbFactory.UserId }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var unbanned = await verifyCtx.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == TestDbFactory.UserId);
        unbanned.Status.Should().Be(UserStatus.Active);
    }

    public void Dispose() => _factory.Dispose();
}
