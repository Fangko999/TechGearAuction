using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.Features.Auctions.Queries;
using TechGearAuction.Application.Features.Users.Queries;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Infrastructure.Data;
using Xunit;

namespace TechGearAuction.Tests.Dashboards;

public class DashboardQueriesTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _mockUserService;

    public DashboardQueriesTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _mockUserService = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task GetTrendingAuctions_ShouldReturnTopAuctionsByBidsCount()
    {
        // Arrange
        var seller = new User { Id = Guid.NewGuid(), Email = "seller@test.com", PasswordHash = "hash", Status = UserStatus.Active, Role = UserRole.User };
        var cat = new Category { Id = Guid.NewGuid(), Name = "Cat" };
        
        var auction1 = new Auction { Id = Guid.NewGuid(), Title = "A1", SellerId = seller.Id, CategoryId = cat.Id, Status = AuctionStatus.Active, EndTime = DateTime.UtcNow.AddDays(1) };
        var auction2 = new Auction { Id = Guid.NewGuid(), Title = "A2", SellerId = seller.Id, CategoryId = cat.Id, Status = AuctionStatus.Active, EndTime = DateTime.UtcNow.AddDays(1) };

        _context.Users.Add(seller);
        _context.Categories.Add(cat);
        _context.Auctions.AddRange(auction1, auction2);
        
        // Auction 1 has 2 bids, Auction 2 has 1 bid
        var bidder = new User { Id = Guid.NewGuid(), Email = "bidder@test.com", PasswordHash = "hash", Status = UserStatus.Active };
        _context.Users.Add(bidder);

        _context.Bids.Add(new Bid { AuctionId = auction1.Id, BidderId = bidder.Id, BidAmount = 100, IpAddress = "127.0.0.1", DeviceHash = "dev1" });
        _context.Bids.Add(new Bid { AuctionId = auction1.Id, BidderId = bidder.Id, BidAmount = 150, IpAddress = "127.0.0.1", DeviceHash = "dev1" });
        _context.Bids.Add(new Bid { AuctionId = auction2.Id, BidderId = bidder.Id, BidAmount = 200, IpAddress = "127.0.0.1", DeviceHash = "dev1" });

        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.UserId).Returns(Guid.Empty);
        var handler = new GetTrendingAuctionsQueryHandler(_context, _mockUserService.Object);

        // Act
        var result = await handler.Handle(new GetTrendingAuctionsQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(auction1.Id, result[0].Id); // A1 should be first because it has 2 bids
        Assert.Equal(auction2.Id, result[1].Id);
    }

    [Fact]
    public async Task GetMyActiveBids_ShouldReturnCorrectIsWinningFlag()
    {
        // Arrange
        var myUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var seller = new User { Id = Guid.NewGuid(), Email = "seller@test.com", PasswordHash = "hash", Status = UserStatus.Active, Role = UserRole.User };
        var me = new User { Id = myUserId, Email = "me@test.com", PasswordHash = "hash", Status = UserStatus.Active };
        var other = new User { Id = otherUserId, Email = "other@test.com", PasswordHash = "hash", Status = UserStatus.Active };
        var cat = new Category { Id = Guid.NewGuid(), Name = "Cat" };

        _context.Users.AddRange(seller, me, other);
        _context.Categories.Add(cat);

        // Auction 1: I am winning
        var auction1 = new Auction { Id = Guid.NewGuid(), Title = "A1", SellerId = seller.Id, CategoryId = cat.Id, Status = AuctionStatus.Active, EndTime = DateTime.UtcNow.AddDays(1) };
        _context.Auctions.Add(auction1);
        _context.Bids.Add(new Bid { AuctionId = auction1.Id, BidderId = otherUserId, BidAmount = 100, IpAddress = "IP", DeviceHash = "Dev" });
        _context.Bids.Add(new Bid { AuctionId = auction1.Id, BidderId = myUserId, BidAmount = 150, IpAddress = "IP", DeviceHash = "Dev" });

        // Auction 2: I am losing
        var auction2 = new Auction { Id = Guid.NewGuid(), Title = "A2", SellerId = seller.Id, CategoryId = cat.Id, Status = AuctionStatus.Active, EndTime = DateTime.UtcNow.AddDays(1) };
        _context.Auctions.Add(auction2);
        _context.Bids.Add(new Bid { AuctionId = auction2.Id, BidderId = myUserId, BidAmount = 100, IpAddress = "IP", DeviceHash = "Dev" });
        _context.Bids.Add(new Bid { AuctionId = auction2.Id, BidderId = otherUserId, BidAmount = 150, IpAddress = "IP", DeviceHash = "Dev" });

        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.UserId).Returns(myUserId);
        var handler = new GetMyActiveBidsQueryHandler(_context, _mockUserService.Object);

        // Act
        var result = await handler.Handle(new GetMyActiveBidsQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        var a1Result = result.First(r => r.AuctionId == auction1.Id);
        var a2Result = result.First(r => r.AuctionId == auction2.Id);

        Assert.True(a1Result.IsWinning);
        Assert.Equal(150, a1Result.MyMaxBid);

        Assert.False(a2Result.IsWinning);
        Assert.Equal(100, a2Result.MyMaxBid);
    }

    [Fact]
    public async Task GetFeedAuctions_ShouldReturnOnlyFollowedSellersAuctions()
    {
        // Arrange
        var myUserId = Guid.NewGuid();
        var followedSellerId = Guid.NewGuid();
        var otherSellerId = Guid.NewGuid();
        
        var me = new User { Id = myUserId, Email = "me@test.com", PasswordHash = "hash", Status = UserStatus.Active };
        var followed = new User { Id = followedSellerId, Email = "f@test.com", PasswordHash = "hash", Status = UserStatus.Active };
        var other = new User { Id = otherSellerId, Email = "o@test.com", PasswordHash = "hash", Status = UserStatus.Active };
        var cat = new Category { Id = Guid.NewGuid(), Name = "Cat" };

        _context.Users.AddRange(me, followed, other);
        _context.Categories.Add(cat);
        _context.UserFollows.Add(new UserFollow { FollowerId = myUserId, FolloweeId = followedSellerId });

        var a1 = new Auction { Id = Guid.NewGuid(), Title = "A1", SellerId = followedSellerId, CategoryId = cat.Id, Status = AuctionStatus.Active, EndTime = DateTime.UtcNow.AddDays(1) };
        var a2 = new Auction { Id = Guid.NewGuid(), Title = "A2", SellerId = otherSellerId, CategoryId = cat.Id, Status = AuctionStatus.Active, EndTime = DateTime.UtcNow.AddDays(1) };
        
        _context.Auctions.AddRange(a1, a2);
        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.UserId).Returns(myUserId);
        var handler = new GetFeedAuctionsQueryHandler(_context, _mockUserService.Object);

        // Act
        var result = await handler.Handle(new GetFeedAuctionsQuery(), CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(a1.Id, result.Items.First().Id);
    }
}

