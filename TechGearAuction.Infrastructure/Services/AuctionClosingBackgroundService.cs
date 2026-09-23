using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Infrastructure.Services;

public class AuctionClosingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionClosingBackgroundService> _logger;

    public AuctionClosingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AuctionClosingBackgroundService> logger)
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
                await ProcessEndedAuctionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing ended auctions.");
            }

            // Polling interval
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessEndedAuctionsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<IAuctionNotificationService>();

        var endedAuctions = await context.Auctions
            .Include(a => a.Bids)
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime <= DateTime.UtcNow)
            .ToListAsync(stoppingToken);

        foreach (var auction in endedAuctions)
        {
            var highestBid = auction.Bids.OrderByDescending(b => b.BidAmount).FirstOrDefault();

            if (highestBid != null)
            {
                auction.WinnerId = highestBid.BidderId;
                auction.Status = AuctionStatus.Completed;
                _logger.LogInformation("Auction {AuctionId} ended. Winner: {WinnerId}, Final Price: {FinalPrice}", auction.Id, highestBid.BidderId, auction.CurrentPrice);
                
                // Create ChatRoom for Winner and Seller
                var chatRoom = new ChatRoom
                {
                    AuctionId = auction.Id,
                    Status = ChatRoomStatus.Active,
                    ExpiresAt = DateTime.UtcNow.AddDays(30)
                };
                context.ChatRooms.Add(chatRoom);

                var sysMessage = new ChatMessage
                {
                    ChatRoom = chatRoom,
                    SenderId = null,
                    Content = $"Phòng chat tự động khởi tạo. Giá chốt: {auction.CurrentPrice:N0}",
                    MessageType = ChatMessageType.SystemText,
                    IsRead = false
                };
                context.ChatMessages.Add(sysMessage);

                // Get winner name for notification
                var winner = await context.Users.FindAsync(new object[] { highestBid.BidderId }, stoppingToken);
                await notificationService.NotifyAuctionEndedAsync(auction.Id, winner?.DisplayName ?? "Anonymous", auction.CurrentPrice);
            }
            else
            {
                // No bids placed
                auction.Status = AuctionStatus.Completed; // Or Cancelled/Unsold
                _logger.LogInformation("Auction {AuctionId} ended with no bids.", auction.Id);
                await notificationService.NotifyAuctionEndedAsync(auction.Id, "No Winner", auction.CurrentPrice);
            }
        }

        if (endedAuctions.Any())
        {
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}

