using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TechGearAuction.Application.Features.Auctions.Commands;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Infrastructure.Services;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Chat;

public class MatchmakingTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IAuctionNotificationService> _notificationMock;
    private readonly Mock<ILogger<AuctionClosingBackgroundService>> _loggerMock;

    public MatchmakingTests()
    {
        _factory = new TestDbFactory();
        _notificationMock = new Mock<IAuctionNotificationService>();
        _loggerMock = new Mock<ILogger<AuctionClosingBackgroundService>>();
    }

    [Fact]
    public async Task AuctionClosingJob_WithWinner_ShouldCreateActiveChatRoom()
    {
        Guid endedId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = endedId,
                Title = "Ended With Bids",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(-2),
                EndTime = DateTime.UtcNow.AddSeconds(-1),
                Status = AuctionStatus.Active,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });
            setupCtx.Bids.Add(new TechGearAuction.Domain.Entities.Bid
            {
                AuctionId = endedId,
                BidderId = TestDbFactory.User2Id,
                BidAmount = 15,
                IpAddress = "1",
                DeviceHash = "1"
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
        var chatRoom = await verifyCtx.ChatRooms.FirstOrDefaultAsync(r => r.AuctionId == endedId);
        chatRoom.Should().NotBeNull();
        chatRoom!.Status.Should().Be(ChatRoomStatus.Active);
        chatRoom.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(29)); // ~30 days
    }

    [Fact]
    public async Task BuyNow_ShouldCreateActiveChatRoom()
    {
        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        var ctx = _factory.CreateContext();
        var handler = new BuyNowCommandHandler(ctx, svc, _notificationMock.Object);

        await handler.Handle(new BuyNowCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            IpAddress = "192.168.1.5",
            DeviceHash = "hash"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var chatRoom = await verifyCtx.ChatRooms.FirstOrDefaultAsync(r => r.AuctionId == TestDbFactory.ActiveAuctionId);
        chatRoom.Should().NotBeNull();
        chatRoom!.Status.Should().Be(ChatRoomStatus.Active);
        chatRoom.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(29));
    }

    public void Dispose() => _factory.Dispose();
}
