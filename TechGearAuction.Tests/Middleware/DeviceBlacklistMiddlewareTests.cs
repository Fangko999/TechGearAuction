using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Infrastructure.Data;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Middleware;

public class DeviceBlacklistMiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DeviceBlacklistMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        SeedBannedDevice();
    }

    private void SeedBannedDevice()
    {
        _factory.CreateClient(); // Initialize Host
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechGearAuction.Application.Interfaces.IAppDbContext>();
        if (!db.BannedDevices.Any(b => b.DeviceHash == "BANNED-DEVICE"))
        {
            db.BannedDevices.Add(new BannedDevice { DeviceHash = "BANNED-DEVICE", Reason = "Test" });
            db.SaveChangesAsync(default).GetAwaiter().GetResult();
        }
    }

    [Fact]
    public async Task Request_WithBannedDeviceHash_ShouldReturn403()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Device-Hash", "BANNED-DEVICE");

        var response = await client.GetAsync("/api/auctions"); // Any route

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("banned");
    }

    [Fact]
    public async Task Request_WithCleanDeviceHash_ShouldPassThrough()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Device-Hash", "CLEAN-DEVICE");

        var response = await client.GetAsync("/api/auctions"); // Public route

        // Should not be 403. Might be 200 or 401 depending on route, but definitely not 403 Forbidden by middleware.
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Request_WithoutDeviceHashHeader_ShouldPassThrough()
    {
        var client = _factory.CreateClient();
        // No header

        var response = await client.GetAsync("/api/auctions"); 

        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Request_ToAppealRoute_ShouldBypassBlacklist()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Device-Hash", "BANNED-DEVICE");

        // The middleware bypasses for "/api/appeals/banned-users"
        var response = await client.GetAsync("/api/appeals/banned-users"); 

        // Not 403
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Forbidden);
    }
}
