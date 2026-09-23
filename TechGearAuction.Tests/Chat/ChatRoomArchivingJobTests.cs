using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Infrastructure.Services;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Chat;

public class ChatRoomArchivingJobTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<ILogger<ChatRoomArchivingBackgroundService>> _loggerMock;

    public ChatRoomArchivingJobTests()
    {
        _factory = new TestDbFactory();
        _loggerMock = new Mock<ILogger<ChatRoomArchivingBackgroundService>>();
    }

    [Fact]
    public async Task ProcessExpiredRooms_ShouldSetStatusToArchived()
    {
        Guid expiredRoomId = Guid.NewGuid();
        Guid activeRoomId = Guid.NewGuid();

        using (var setupCtx = _factory.CreateContext())
        {
            var activeAuction1 = new TechGearAuction.Domain.Entities.Auction { Id = Guid.NewGuid(), Title = "A1", StartPrice = 1, BidIncrement = 1, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddDays(1), SellerId = TestDbFactory.UserId, CategoryId = TestDbFactory.ChildCategoryId };
            var activeAuction2 = new TechGearAuction.Domain.Entities.Auction { Id = Guid.NewGuid(), Title = "A2", StartPrice = 1, BidIncrement = 1, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddDays(1), SellerId = TestDbFactory.UserId, CategoryId = TestDbFactory.ChildCategoryId };
            
            setupCtx.Auctions.AddRange(activeAuction1, activeAuction2);

            setupCtx.ChatRooms.Add(new TechGearAuction.Domain.Entities.ChatRoom
            {
                Id = expiredRoomId,
                AuctionId = activeAuction1.Id,
                Status = ChatRoomStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(-1) // Expired
            });
            
            setupCtx.ChatRooms.Add(new TechGearAuction.Domain.Entities.ChatRoom
            {
                Id = activeRoomId,
                AuctionId = activeAuction2.Id,
                Status = ChatRoomStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(10) // Not expired
            });

            await setupCtx.SaveChangesAsync();
        }

        var services = new ServiceCollection();
        services.AddScoped<TechGearAuction.Application.Interfaces.IAppDbContext>(_ => _factory.CreateContext());
        var provider = services.BuildServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(s => s.CreateScope()).Returns(provider.CreateScope());

        var service = new ChatRoomArchivingBackgroundService(scopeFactoryMock.Object, _loggerMock.Object);
        var method = typeof(ChatRoomArchivingBackgroundService).GetMethod("ProcessExpiredRoomsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)method!.Invoke(service, new object[] { CancellationToken.None })!;

        using var verifyCtx = _factory.CreateContext();
        
        var expiredRoom = await verifyCtx.ChatRooms.FindAsync(expiredRoomId);
        expiredRoom!.Status.Should().Be(ChatRoomStatus.Archived);

        var activeRoom = await verifyCtx.ChatRooms.FindAsync(activeRoomId);
        activeRoom!.Status.Should().Be(ChatRoomStatus.Active);
    }

    public void Dispose() => _factory.Dispose();
}
