using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Reports.Commands;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Reports;

public class CreateReportTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public CreateReportTests()
    {
        _factory = new TestDbFactory();
    }

    [Fact]
    public async Task CreateReport_WithValidData_ShouldPersistReport()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CreateReportCommandHandler(ctx, svc);

        var reportId = await handler.Handle(new CreateReportCommand
        {
            ReportedUserId = TestDbFactory.User2Id,
            AuctionId = TestDbFactory.ActiveAuctionId,
            Type = ReportType.Scam,
            Description = "Fake item"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var report = await verifyCtx.Reports.FindAsync(reportId);
        
        report.Should().NotBeNull();
        report!.ReporterId.Should().Be(TestDbFactory.UserId);
        report.ReportedUserId.Should().Be(TestDbFactory.User2Id);
        report.AuctionId.Should().Be(TestDbFactory.ActiveAuctionId);
        report.Type.Should().Be(ReportType.Scam);
        report.Description.Should().Be("Fake item");
        report.Status.Should().Be(ReportStatus.Pending);
    }

    [Fact]
    public async Task CreateReport_OnSelf_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CreateReportCommandHandler(ctx, svc);

        var act = () => handler.Handle(new CreateReportCommand
        {
            ReportedUserId = TestDbFactory.UserId,
            Type = ReportType.Scam
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*cannot report yourself*");
    }

    [Fact]
    public async Task CreateReport_WhenReportedUserNotFound_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CreateReportCommandHandler(ctx, svc);

        var act = () => handler.Handle(new CreateReportCommand
        {
            ReportedUserId = Guid.NewGuid(), // does not exist
            Type = ReportType.Scam
        }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*user not found*");
    }
    
    [Fact]
    public async Task CreateReport_WhenAuctionNotFound_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CreateReportCommandHandler(ctx, svc);

        var act = () => handler.Handle(new CreateReportCommand
        {
            ReportedUserId = TestDbFactory.User2Id,
            AuctionId = Guid.NewGuid(), // does not exist
            Type = ReportType.Scam
        }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Auction not found*");
    }

    public void Dispose() => _factory.Dispose();
}
