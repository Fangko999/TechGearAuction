using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Admin.Commands;
using TechGearAuction.Application.Features.Admin.Queries;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Exceptions;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Admin;

public class AdminBlacklistTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public AdminBlacklistTests()
    {
        _factory = new TestDbFactory();
    }

    [Fact]
    public async Task GetBlacklist_ShouldReturnAllBannedDevices()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.BannedDevices.Add(new BannedDevice { DeviceHash = "banned-hash-1", Reason = "spam", BannedAt = DateTime.UtcNow });
            await setupCtx.SaveChangesAsync();
        }

        var ctx = _factory.CreateContext();
        var handler = new GetBlacklistsQueryHandler(ctx);

        var result = await handler.Handle(new GetBlacklistsQuery(), CancellationToken.None);
        
        result.Items.Should().Contain(d => d.DeviceHash == "banned-hash-1");
    }

    [Fact]
    public async Task RemoveFromBlacklist_ShouldDeleteDeviceAndCreateAuditLog()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.BannedDevices.Add(new BannedDevice { DeviceHash = "banned-hash-2", Reason = "spam", BannedAt = DateTime.UtcNow });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var ctx = _factory.CreateContext();
        var handler = new RemoveFromBlacklistCommandHandler(ctx, svc);

        await handler.Handle(new RemoveFromBlacklistCommand { DeviceHash = "banned-hash-2" }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var device = await verifyCtx.BannedDevices.FirstOrDefaultAsync(b => b.DeviceHash == "banned-hash-2");
        device.Should().BeNull();

        var log = await verifyCtx.AdminAuditLogs.FirstOrDefaultAsync(l => l.Action == "RemoveFromBlacklist");
        log.Should().NotBeNull();
        log!.Details.Should().Contain("banned-hash-2");
    }

    [Fact]
    public async Task RemoveFromBlacklist_WhenDeviceNotFound_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var ctx = _factory.CreateContext();
        var handler = new RemoveFromBlacklistCommandHandler(ctx, svc);

        var act = () => handler.Handle(new RemoveFromBlacklistCommand { DeviceHash = "non-existent" }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*not found in blacklist*");
    }

    public void Dispose() => _factory.Dispose();
}
