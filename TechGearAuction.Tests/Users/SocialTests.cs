using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Users.Commands;
using TechGearAuction.Application.Features.Users.Queries;
using TechGearAuction.Application.Interfaces;
using Moq;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Users;

public class SocialTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IAuctionNotificationService> _notificationMock;

    public SocialTests()
    {
        _factory = new TestDbFactory();
        _notificationMock = new Mock<IAuctionNotificationService>();
    }

    // ─── Follow Tests ────────────────────────────────────────────────────────

    [Fact]
    public async Task ToggleFollow_ShouldFollowOnFirstCallAndUnfollowOnSecond()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);

        // First call → Follow
        var ctx1 = _factory.CreateContext();
        var isFollowing = await new ToggleUserFollowCommandHandler(ctx1, svc)
            .Handle(new ToggleUserFollowCommand { FolloweeId = TestDbFactory.User2Id }, CancellationToken.None);
        isFollowing.Should().BeTrue();

        // Second call → Unfollow
        var ctx2 = _factory.CreateContext();
        var isFollowing2 = await new ToggleUserFollowCommandHandler(ctx2, svc)
            .Handle(new ToggleUserFollowCommand { FolloweeId = TestDbFactory.User2Id }, CancellationToken.None);
        isFollowing2.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleFollow_OnSelf_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new ToggleUserFollowCommandHandler(ctx, svc);

        var act = () => handler.Handle(
            new ToggleUserFollowCommand { FolloweeId = TestDbFactory.UserId }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetFollowing_ShouldReturnPaginatedResult()
    {
        // Setup: UserId follows User2Id
        var followSvc = new MockCurrentUserService(TestDbFactory.UserId);
        var followCtx = _factory.CreateContext();
        await new ToggleUserFollowCommandHandler(followCtx, followSvc)
            .Handle(new ToggleUserFollowCommand { FolloweeId = TestDbFactory.User2Id }, CancellationToken.None);

        var ctx = _factory.CreateContext();
        var handler = new GetMyFollowingQueryHandler(ctx, followSvc);
        var result = await handler.Handle(new GetMyFollowingQuery { PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().BeGreaterThan(0);
        result.Items.Should().Contain(u => u.Id == TestDbFactory.User2Id);
    }

    // ─── Block Tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ToggleBlock_ShouldBreakFollowRelationshipBothWays()
    {
        var user1Svc = new MockCurrentUserService(TestDbFactory.UserId);
        var user2Svc = new MockCurrentUserService(TestDbFactory.User2Id);

        // Setup: both follow each other
        await new ToggleUserFollowCommandHandler(_factory.CreateContext(), user1Svc)
            .Handle(new ToggleUserFollowCommand { FolloweeId = TestDbFactory.User2Id }, CancellationToken.None);
        await new ToggleUserFollowCommandHandler(_factory.CreateContext(), user2Svc)
            .Handle(new ToggleUserFollowCommand { FolloweeId = TestDbFactory.UserId }, CancellationToken.None);

        // UserId blocks User2Id
        await new ToggleUserBlockCommandHandler(_factory.CreateContext(), user1Svc, _notificationMock.Object)
            .Handle(new ToggleUserBlockCommand { BlockedId = TestDbFactory.User2Id }, CancellationToken.None);

        using var ctx = _factory.CreateContext();
        // Both follow relationships should be soft-deleted
        var follows = await ctx.UserFollows.IgnoreQueryFilters()
            .Where(f => (f.FollowerId == TestDbFactory.UserId && f.FolloweeId == TestDbFactory.User2Id) ||
                        (f.FollowerId == TestDbFactory.User2Id && f.FolloweeId == TestDbFactory.UserId))
            .ToListAsync();

        follows.Should().NotBeEmpty();
        follows.All(f => f.DeletedAt != null).Should().BeTrue("blocking must soft-delete follow in both directions");
    }

    [Fact]
    public async Task ToggleBlock_OnSelf_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new ToggleUserBlockCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(
            new ToggleUserBlockCommand { BlockedId = TestDbFactory.UserId }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetBlockedUsers_ShouldReturnCorrectList()
    {
        // Setup: UserId blocks User2Id
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        await new ToggleUserBlockCommandHandler(_factory.CreateContext(), svc, _notificationMock.Object)
            .Handle(new ToggleUserBlockCommand { BlockedId = TestDbFactory.User2Id }, CancellationToken.None);

        var ctx = _factory.CreateContext();
        var handler = new GetMyBlockedUsersQueryHandler(ctx, svc);
        var result = await handler.Handle(new GetMyBlockedUsersQuery { PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().Contain(u => u.Id == TestDbFactory.User2Id);
    }

    [Fact]
    public async Task ToggleBlock_WhenBlockedUserIsTopBidder_ShouldCancelBidAndRollbackPrice()
    {
        Guid auctionId = Guid.NewGuid();
        Guid otherBidderId = TestDbFactory.AdminId;

        using (var setupCtx = _factory.CreateContext())
        {
            var auction = new TechGearAuction.Domain.Entities.Auction
            {
                Id = auctionId,
                Title = "Test Auction",
                StartPrice = 100,
                CurrentPrice = 300,
                Status = TechGearAuction.Domain.Enums.AuctionStatus.Active,
                SellerId = TestDbFactory.UserId, // Seller is UserId
                CategoryId = TestDbFactory.ChildCategoryId,
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(1)
            };
            setupCtx.Auctions.Add(auction);

            // Admin bids 200
            setupCtx.Bids.Add(new TechGearAuction.Domain.Entities.Bid
            {
                AuctionId = auctionId,
                BidderId = otherBidderId,
                BidAmount = 200,
                IpAddress = "1", DeviceHash = "1"
            });

            // User2 bids 300 (Top Bidder)
            setupCtx.Bids.Add(new TechGearAuction.Domain.Entities.Bid
            {
                AuctionId = auctionId,
                BidderId = TestDbFactory.User2Id,
                BidAmount = 300,
                IpAddress = "2", DeviceHash = "2"
            });

            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new ToggleUserBlockCommandHandler(ctx, svc, _notificationMock.Object);

        // Seller blocks User2
        await handler.Handle(new ToggleUserBlockCommand { BlockedId = TestDbFactory.User2Id }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var auctionVerify = await verifyCtx.Auctions.FindAsync(auctionId);
        auctionVerify!.CurrentPrice.Should().Be(200);

        var topBid = await verifyCtx.Bids.FirstOrDefaultAsync(b => b.BidderId == TestDbFactory.User2Id);
        topBid!.IsCanceled.Should().BeTrue();

        var adminBid = await verifyCtx.Bids.FirstOrDefaultAsync(b => b.BidderId == otherBidderId);
        adminBid!.IsCanceled.Should().BeFalse();

        _notificationMock.Verify(n => n.NotifyPriceUpdateAsync(auctionId, 200), Times.Once);
    }

    public void Dispose() => _factory.Dispose();
}

