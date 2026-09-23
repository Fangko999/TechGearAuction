using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.Common.Exceptions;
using TechGearAuction.Application.Features.Auctions.Commands;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Auctions;

public class PlaceBidTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IAuctionNotificationService> _notificationMock;

    public PlaceBidTests()
    {
        _factory = new TestDbFactory();
        _notificationMock = new Mock<IAuctionNotificationService>();
    }

    [Fact]
    public async Task PlaceBid_WithSameDeviceHashAsAnotherBidder_ShouldLogSuspiciousActivity()
    {
        // Add a bid from Admin with DeviceHash = "SpamDevice"
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Bids.Add(new TechGearAuction.Domain.Entities.Bid
            {
                AuctionId = TestDbFactory.ActiveAuctionId,
                BidderId = TestDbFactory.AdminId, // someone else
                BidAmount = 15,
                IpAddress = "192.168.1.100",
                DeviceHash = "SpamDevice"
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new PlaceBidCommandHandler(ctx, svc, _notificationMock.Object);

        await handler.Handle(new PlaceBidCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            BidAmount = 1050, // ActiveAuctionId CurrentPrice is 1000 + 50 increment
            IpAddress = "10.0.0.1", // different IP
            DeviceHash = "SpamDevice" // same DeviceHash
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var activity = await verifyCtx.SuspiciousActivities.FirstOrDefaultAsync(a => a.BidderId == TestDbFactory.User2Id);
        
        activity.Should().NotBeNull();
        activity!.Reason.Should().Contain("Device Hash matches another Bidder");
    }

    [Fact]
    public async Task PlaceBid_WithSameDeviceHashAsSeller_ShouldLogSuspiciousActivity()
    {
        // Update seller with DeviceHash = "SellerDevice"
        using (var setupCtx = _factory.CreateContext())
        {
            var seller = await setupCtx.Users.FindAsync(TestDbFactory.UserId);
            seller!.LastLoginDeviceHash = "SellerDevice";
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new PlaceBidCommandHandler(ctx, svc, _notificationMock.Object);

        await handler.Handle(new PlaceBidCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            BidAmount = 1050,
            IpAddress = "10.0.0.1",
            DeviceHash = "SellerDevice" // match seller
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var activity = await verifyCtx.SuspiciousActivities.FirstOrDefaultAsync(a => a.BidderId == TestDbFactory.User2Id);
        
        activity.Should().NotBeNull();
        activity!.Reason.Should().Contain("Bidder Device Hash matches Seller");
    }

    [Fact]
    public async Task PlaceBid_OnNotActiveAuction_ShouldThrow()
    {
        Guid draftId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = draftId,
                Title = "Draft",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Status = AuctionStatus.Draft,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new PlaceBidCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new PlaceBidCommand
        {
            AuctionId = draftId,
            BidAmount = 15,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*not currently active*");
    }

    [Fact]
    public async Task PlaceBid_OnEndedAuction_ShouldThrow()
    {
        Guid endedId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = endedId,
                Title = "Ended",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(-2),
                EndTime = DateTime.UtcNow.AddDays(-1),
                Status = AuctionStatus.Active, // Status is Active but EndTime is passed
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new PlaceBidCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new PlaceBidCommand
        {
            AuctionId = endedId,
            BidAmount = 15,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*already ended*");
    }

    [Fact]
    public async Task PlaceBid_OnOwnAuction_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId); // Same as SellerId of ActiveAuctionId
        var ctx = _factory.CreateContext();
        var handler = new PlaceBidCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new PlaceBidCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            BidAmount = 2000,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*your own auction*");
    }

    [Fact]
    public async Task PlaceBid_WhenBlockedBySeller_ShouldThrow()
    {
        // Block User2 by UserId (Seller)
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.UserBlocks.Add(new TechGearAuction.Domain.Entities.UserBlock
            {
                BlockerId = TestDbFactory.UserId,
                BlockedId = TestDbFactory.User2Id
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new PlaceBidCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new PlaceBidCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            BidAmount = 1500,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*not allowed to bid*");
    }

    [Fact]
    public async Task PlaceBid_WithAmountLowerThanMinimum_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new PlaceBidCommandHandler(ctx, svc, _notificationMock.Object);

        // ActiveAuctionId has CurrentPrice=1000, BidIncrement=50. Min bid = 1050.
        var act = () => handler.Handle(new PlaceBidCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            BidAmount = 1040,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*must be at least*");
    }

    [Fact]
    public async Task PlaceBid_WithAmountEqualOrHigherThanBuyNow_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new PlaceBidCommandHandler(ctx, svc, _notificationMock.Object);

        // BuyNow is 2000
        var act = () => handler.Handle(new PlaceBidCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            BidAmount = 2500,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*cannot be greater than or equal to the Buy Now price*");
    }

    [Fact]
    public async Task PlaceBid_WithValidAmount_ShouldUpdatePriceAndNotifyAndSaveDeviceHash()
    {
        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new PlaceBidCommandHandler(ctx, svc, _notificationMock.Object);

        await handler.Handle(new PlaceBidCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            BidAmount = 1100,
            IpAddress = "192.168.1.1",
            DeviceHash = "valid-hash-123"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var auction = await verifyCtx.Auctions.FindAsync(TestDbFactory.ActiveAuctionId);
        auction!.CurrentPrice.Should().Be(1100);

        var bid = await verifyCtx.Bids.FirstOrDefaultAsync(b => b.AuctionId == TestDbFactory.ActiveAuctionId);
        bid.Should().NotBeNull();
        bid!.BidAmount.Should().Be(1100);
        bid.IpAddress.Should().Be("192.168.1.1");
        bid.DeviceHash.Should().Be("valid-hash-123");

        _notificationMock.Verify(n => n.NotifyNewBidAsync(TestDbFactory.ActiveAuctionId, It.IsAny<string>(), 1100, It.IsAny<DateTime>()), Times.Once);
        _notificationMock.Verify(n => n.NotifyPriceUpdateAsync(TestDbFactory.ActiveAuctionId, 1100), Times.Once);
    }

    [Fact]
    public async Task PlaceBid_WithConcurrencyConflict_ShouldThrowSpecificException()
    {
        // Mock the context to throw DbUpdateConcurrencyException
        var ctxMock = new Mock<IAppDbContext>();
        ctxMock.Setup(c => c.Auctions).Returns(_factory.CreateContext().Auctions);
        ctxMock.Setup(c => c.Bids).Returns(_factory.CreateContext().Bids);
        ctxMock.Setup(c => c.Users).Returns(_factory.CreateContext().Users);
        ctxMock.Setup(c => c.UserBlocks).Returns(_factory.CreateContext().UserBlocks);

        ctxMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var handler = new PlaceBidCommandHandler(ctxMock.Object, svc, _notificationMock.Object);

        var act = () => handler.Handle(new PlaceBidCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            BidAmount = 1100,
            IpAddress = "127.0.0.1",
            DeviceHash = "abc"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyException>().WithMessage("*exact same time*");
    }

    public void Dispose() => _factory.Dispose();
}

