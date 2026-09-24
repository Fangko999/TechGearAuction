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

public class AuctionActivationJobTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<ILogger<AuctionActivationBackgroundService>> _loggerMock;

    public AuctionActivationJobTests()
    {
        _factory = new TestDbFactory();
        _loggerMock = new Mock<ILogger<AuctionActivationBackgroundService>>();
    }

    [Fact]
    public async Task ProcessScheduledAuctions_ShouldActivateAuctionsWhenStartTimeReached()
    {
        Guid scheduledId = Guid.NewGuid();
        Guid futureId = Guid.NewGuid();

        using (var setupCtx = _factory.CreateContext())
        {
            // Auction that should be activated
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = scheduledId,
                Title = "Should Activate",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddMinutes(-5), // Start time has passed
                EndTime = DateTime.UtcNow.AddDays(1),
                Status = AuctionStatus.Scheduled,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });
            
            // Auction that should NOT be activated yet
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = futureId,
                Title = "Should Remain Scheduled",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddMinutes(5), // Start time is in the future
                EndTime = DateTime.UtcNow.AddDays(1),
                Status = AuctionStatus.Scheduled,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });

            await setupCtx.SaveChangesAsync();
        }

        var services = new ServiceCollection();
        services.AddScoped<IAppDbContext>(_ => _factory.CreateContext());
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(s => s.CreateScope()).Returns(provider.CreateScope());

        var service = new AuctionActivationBackgroundService(scopeFactoryMock.Object, _loggerMock.Object);

        // Reflection to call private method
        var method = typeof(AuctionActivationBackgroundService).GetMethod("ProcessScheduledAuctionsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)method!.Invoke(service, new object[] { CancellationToken.None })!;

        using var verifyCtx = _factory.CreateContext();
        
        var activatedAuction = await verifyCtx.Auctions.FindAsync(scheduledId);
        activatedAuction!.Status.Should().Be(AuctionStatus.Active);
        
        var futureAuction = await verifyCtx.Auctions.FindAsync(futureId);
        futureAuction!.Status.Should().Be(AuctionStatus.Scheduled);
    }

    public void Dispose() => _factory.Dispose();
}
