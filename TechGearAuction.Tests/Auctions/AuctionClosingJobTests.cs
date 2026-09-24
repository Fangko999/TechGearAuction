using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Infrastructure.Services;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Auctions;

public class AuctionClosingJobTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IAuctionNotificationService> _notificationMock;
    private readonly Mock<ILogger<AuctionClosingBackgroundService>> _loggerMock;

    public AuctionClosingJobTests()
    {
        _factory = new TestDbFactory();
        _notificationMock = new Mock<IAuctionNotificationService>();
        _loggerMock = new Mock<ILogger<AuctionClosingBackgroundService>>();
    }

    [Fact]
    public async Task ProcessEndedAuctions_WithBids_ShouldAssignWinnerAndCompleteAndSetUpdatedAt()
    {
        Guid endedId = Guid.NewGuid();
        var oldDate = DateTime.UtcNow.AddDays(-10);

        using (var setupCtx = _factory.CreateContext())
        {
            var auction = new TechGearAuction.Domain.Entities.Auction
            {
                Id = endedId,
                Title = "Ended With Bids",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(-2),
                EndTime = DateTime.UtcNow.AddSeconds(-1), // Just ended
                Status = AuctionStatus.Active,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId,
                CreatedAt = oldDate,
                UpdatedAt = oldDate // Set to old date
            };
            setupCtx.Auctions.Add(auction);
            
            // Add bids
            setupCtx.Bids.Add(new TechGearAuction.Domain.Entities.Bid
            {
                AuctionId = endedId,
                BidderId = TestDbFactory.User2Id,
                BidAmount = 15,
                IpAddress = "1",
                DeviceHash = "1"
            });
            setupCtx.Bids.Add(new TechGearAuction.Domain.Entities.Bid
            {
                AuctionId = endedId,
                BidderId = TestDbFactory.AdminId,
                BidAmount = 25, // Highest bid
                IpAddress = "2",
                DeviceHash = "2"
            });

            await setupCtx.SaveChangesAsync();

            // Override the UpdatedAt manually for setup because AppDbContext might have changed it during SaveChanges
            auction.UpdatedAt = oldDate;
            setupCtx.Entry(auction).Property(x => x.UpdatedAt).IsModified = true;
            await setupCtx.SaveChangesAsync();
        }

        var services = new ServiceCollection();
        services.AddScoped<IAppDbContext>(_ => _factory.CreateContext());
        services.AddScoped<IAuctionNotificationService>(_ => _notificationMock.Object);
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(s => s.CreateScope()).Returns(provider.CreateScope());

        var service = new AuctionClosingBackgroundService(scopeFactoryMock.Object, _loggerMock.Object);

        // Reflection to call private method
        var method = typeof(AuctionClosingBackgroundService).GetMethod("ProcessEndedAuctionsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)method!.Invoke(service, new object[] { CancellationToken.None })!;

        using var verifyCtx = _factory.CreateContext();
        var auctionVerify = await verifyCtx.Auctions.FindAsync(endedId);
        
        auctionVerify!.Status.Should().Be(AuctionStatus.Completed);
        auctionVerify.WinnerId.Should().Be(TestDbFactory.AdminId);
        auctionVerify.UpdatedAt.Should().BeAfter(oldDate.AddDays(1), "UpdatedAt must be modified by the worker");

        _notificationMock.Verify(n => n.NotifyAuctionEndedAsync(endedId, It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task ProcessEndedAuctions_WithNoBids_ShouldCompleteWithoutWinnerAndSetUpdatedAt()
    {
        Guid endedId = Guid.NewGuid();
        var oldDate = DateTime.UtcNow.AddDays(-10);

        using (var setupCtx = _factory.CreateContext())
        {
            var auction = new TechGearAuction.Domain.Entities.Auction
            {
                Id = endedId,
                Title = "Ended No Bids",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(-2),
                EndTime = DateTime.UtcNow.AddSeconds(-1),
                Status = AuctionStatus.Active,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            };
            setupCtx.Auctions.Add(auction);
            await setupCtx.SaveChangesAsync();
            
            auction.UpdatedAt = oldDate;
            setupCtx.Entry(auction).Property(x => x.UpdatedAt).IsModified = true;
            await setupCtx.SaveChangesAsync();
        }

        var services = new ServiceCollection();
        services.AddScoped<IAppDbContext>(_ => _factory.CreateContext());
        services.AddScoped<IAuctionNotificationService>(_ => _notificationMock.Object);
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(s => s.CreateScope()).Returns(provider.CreateScope());

        var service = new AuctionClosingBackgroundService(scopeFactoryMock.Object, _loggerMock.Object);

        var method = typeof(AuctionClosingBackgroundService).GetMethod("ProcessEndedAuctionsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)method!.Invoke(service, new object[] { CancellationToken.None })!;

        using var verifyCtx = _factory.CreateContext();
        var auctionVerify = await verifyCtx.Auctions.FindAsync(endedId);
        
        auctionVerify!.Status.Should().Be(AuctionStatus.Cancelled);
        auctionVerify.WinnerId.Should().BeNull();
        auctionVerify.UpdatedAt.Should().BeAfter(oldDate.AddDays(1), "UpdatedAt must be modified by the worker even with no bids");

        _notificationMock.Verify(n => n.NotifyAuctionEndedAsync(endedId, "No Winner", It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public async Task ProcessEndedAuctions_WithHighestBidCanceled_ShouldAssignWinnerToSecondHighest()
    {
        Guid endedId = Guid.NewGuid();

        using (var setupCtx = _factory.CreateContext())
        {
            var auction = new TechGearAuction.Domain.Entities.Auction
            {
                Id = endedId,
                Title = "Ended With Canceled Bid",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(-2),
                EndTime = DateTime.UtcNow.AddSeconds(-1),
                Status = AuctionStatus.Active,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            };
            setupCtx.Auctions.Add(auction);
            
            // Add bids
            setupCtx.Bids.Add(new TechGearAuction.Domain.Entities.Bid
            {
                AuctionId = endedId,
                BidderId = TestDbFactory.User2Id,
                BidAmount = 15, // Second highest, valid
                IpAddress = "1",
                DeviceHash = "1",
                IsCanceled = false
            });
            setupCtx.Bids.Add(new TechGearAuction.Domain.Entities.Bid
            {
                AuctionId = endedId,
                BidderId = TestDbFactory.AdminId,
                BidAmount = 25, // Highest bid, but canceled
                IpAddress = "2",
                DeviceHash = "2",
                IsCanceled = true
            });

            await setupCtx.SaveChangesAsync();
        }

        var services = new ServiceCollection();
        services.AddScoped<IAppDbContext>(_ => _factory.CreateContext());
        services.AddScoped<IAuctionNotificationService>(_ => _notificationMock.Object);
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(s => s.CreateScope()).Returns(provider.CreateScope());

        var service = new AuctionClosingBackgroundService(scopeFactoryMock.Object, _loggerMock.Object);

        var method = typeof(AuctionClosingBackgroundService).GetMethod("ProcessEndedAuctionsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)method!.Invoke(service, new object[] { CancellationToken.None })!;

        using var verifyCtx = _factory.CreateContext();
        var auctionVerify = await verifyCtx.Auctions.FindAsync(endedId);
        
        auctionVerify!.Status.Should().Be(AuctionStatus.Completed);
        auctionVerify.WinnerId.Should().Be(TestDbFactory.User2Id, "Winner should be the highest VALID bid (IsCanceled = false)");
    }

    public void Dispose() => _factory.Dispose();
}

