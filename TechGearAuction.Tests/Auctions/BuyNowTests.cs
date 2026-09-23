using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.Common.Exceptions;
using TechGearAuction.Application.Features.Auctions.Commands;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Auctions;

public class BuyNowTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IAuctionNotificationService> _notificationMock;

    public BuyNowTests()
    {
        _factory = new TestDbFactory();
        _notificationMock = new Mock<IAuctionNotificationService>();
    }

    [Fact]
    public async Task BuyNow_WhenValid_ShouldCreateBidAndCompleteAuction()
    {
        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new BuyNowCommandHandler(ctx, svc, _notificationMock.Object);

        await handler.Handle(new BuyNowCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            IpAddress = "192.168.1.5",
            DeviceHash = "hash-buy-now"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var auction = await verifyCtx.Auctions.FindAsync(TestDbFactory.ActiveAuctionId);
        auction!.Status.Should().Be(AuctionStatus.Completed);
        auction.CurrentPrice.Should().Be(2000); // BuyNow price
        auction.WinnerId.Should().Be(TestDbFactory.User2Id);

        var bid = await verifyCtx.Bids.FirstOrDefaultAsync(b => b.AuctionId == TestDbFactory.ActiveAuctionId && b.BidAmount == 2000);
        bid.Should().NotBeNull();
        bid!.IpAddress.Should().Be("192.168.1.5");
        bid.DeviceHash.Should().Be("hash-buy-now");

        _notificationMock.Verify(n => n.NotifyAuctionEndedAsync(TestDbFactory.ActiveAuctionId, It.IsAny<string>(), 2000), Times.Once);
    }

    [Fact]
    public async Task BuyNow_OnOwnAuction_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new BuyNowCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new BuyNowCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*your own auction*");
    }

    [Fact]
    public async Task BuyNow_WithoutBuyNowPrice_ShouldThrow()
    {
        Guid noBuyNowId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = noBuyNowId,
                Title = "No Buy Now",
                StartPrice = 10,
                BidIncrement = 1,
                BuyNowPrice = null,
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(1),
                Status = AuctionStatus.Active,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new BuyNowCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new BuyNowCommand
        {
            AuctionId = noBuyNowId,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*does not have a Buy Now option*");
    }

    [Fact]
    public async Task BuyNow_WithConcurrencyConflict_ShouldThrowSpecificException()
    {
        var ctxMock = new Mock<IAppDbContext>();
        ctxMock.Setup(c => c.Auctions).Returns(_factory.CreateContext().Auctions);
        ctxMock.Setup(c => c.Bids).Returns(_factory.CreateContext().Bids);
        ctxMock.Setup(c => c.Users).Returns(_factory.CreateContext().Users);
        ctxMock.Setup(c => c.UserBlocks).Returns(_factory.CreateContext().UserBlocks);
        ctxMock.Setup(c => c.ChatRooms).Returns(_factory.CreateContext().ChatRooms);
        ctxMock.Setup(c => c.ChatMessages).Returns(_factory.CreateContext().ChatMessages);

        ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var handler = new BuyNowCommandHandler(ctxMock.Object, svc, _notificationMock.Object);

        var act = () => handler.Handle(new BuyNowCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyException>().WithMessage("*exact same time*");
    }

    public void Dispose() => _factory.Dispose();
}

