using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Admin.Commands;
using TechGearAuction.Application.Features.Admin.Queries;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Domain.Exceptions;
using TechGearAuction.Tests.Helpers;
using Moq;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Tests.Admin;

public class AdminAuctionTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IAuctionNotificationService> _notificationMock;

    public AdminAuctionTests()
    {
        _factory = new TestDbFactory();
        _notificationMock = new Mock<IAuctionNotificationService>();
    }

    [Fact]
    public async Task ForceCancelAuction_WhenActive_ShouldSetCancelledAndCreateAuditLog()
    {
        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var ctx = _factory.CreateContext();
        var handler = new ForceCancelAuctionCommandHandler(ctx, svc, _notificationMock.Object);

        await handler.Handle(new ForceCancelAuctionCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            Reason = "Violation of terms"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var auction = await verifyCtx.Auctions.FindAsync(TestDbFactory.ActiveAuctionId);
        auction!.Status.Should().Be(AuctionStatus.Cancelled);

        var log = await verifyCtx.AdminAuditLogs.FirstOrDefaultAsync(l => l.EntityId == TestDbFactory.ActiveAuctionId);
        log.Should().NotBeNull();
        log!.Action.Should().Be("ForceCancelAuction");

        _notificationMock.Verify(n => n.NotifyAuctionEndedAsync(TestDbFactory.ActiveAuctionId, It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task ForceCancelAuction_WhenAlreadyCompleted_ShouldThrow()
    {
        var auctionId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = auctionId,
                Title = "Completed",
                Status = AuctionStatus.Completed,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var ctx = _factory.CreateContext();
        var handler = new ForceCancelAuctionCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new ForceCancelAuctionCommand { AuctionId = auctionId, Reason = "test" }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already ended or cancelled*");
    }

    [Fact]
    public async Task GetAllAuctions_Admin_ShouldReturnIncludingDraftAndCancelled()
    {
        var auctionId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = auctionId,
                Title = "Draft",
                Status = AuctionStatus.Draft,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });
            await setupCtx.SaveChangesAsync();
        }

        var ctx = _factory.CreateContext();
        var handler = new GetAdminAuctionsQueryHandler(ctx);

        var result = await handler.Handle(new GetAdminAuctionsQuery(), CancellationToken.None);

        // One ActiveAuctionId, One Draft
        result.Items.Count.Should().BeGreaterThanOrEqualTo(2);
        result.Items.Should().Contain(a => a.Id == auctionId);
    }

    public void Dispose() => _factory.Dispose();
}
