using FluentAssertions;
using TechGearAuction.Application.Features.Admin.Queries;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Admin;

public class AdminMetricsTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public AdminMetricsTests()
    {
        _factory = new TestDbFactory();
    }

    [Fact]
    public async Task GetOverviewMetrics_ShouldReturnCorrectCounts()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Reports.Add(new Report { Id = Guid.NewGuid(), ReporterId = TestDbFactory.UserId, ReportedUserId = TestDbFactory.User2Id, Status = ReportStatus.Pending, Type = ReportType.Scam });
            await setupCtx.SaveChangesAsync();
        }

        var ctx = _factory.CreateContext();
        var handler = new GetMetricsOverviewQueryHandler(ctx);

        var result = await handler.Handle(new GetMetricsOverviewQuery(), CancellationToken.None);

        result.TotalUsers.Should().Be(3); // Admin, UserId, User2Id
        result.ActiveAuctions.Should().Be(1); // ActiveAuctionId
        result.PendingReports.Should().Be(1); // Added above
        result.TotalRevenue.Should().Be(0); // No completed auctions
    }

    public void Dispose() => _factory.Dispose();
}
