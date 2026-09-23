using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Infrastructure.Services;

public class ChatRoomArchivingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ChatRoomArchivingBackgroundService> _logger;

    public ChatRoomArchivingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ChatRoomArchivingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredRoomsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while archiving chat rooms.");
            }

            // Run once every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task ProcessExpiredRoomsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var expiredRooms = await context.ChatRooms
            .Where(r => r.Status == ChatRoomStatus.Active && r.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(stoppingToken);

        if (expiredRooms.Any())
        {
            foreach (var room in expiredRooms)
            {
                room.Status = ChatRoomStatus.Archived;
                _logger.LogInformation("ChatRoom {ChatRoomId} for Auction {AuctionId} has been archived.", room.Id, room.AuctionId);
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
