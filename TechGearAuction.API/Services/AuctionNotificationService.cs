using Microsoft.AspNetCore.SignalR;
using TechGearAuction.API.Hubs;
using TechGearAuction.Application.Hubs;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.API.Services;

public class AuctionNotificationService : IAuctionNotificationService
{
    private readonly IHubContext<AuctionHub, IAuctionHub> _hubContext;

    public AuctionNotificationService(IHubContext<AuctionHub, IAuctionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewBidAsync(Guid auctionId, string bidderName, decimal amount, DateTime time)
    {
        await _hubContext.Clients.Group(auctionId.ToString()).ReceiveNewBid(bidderName, amount, time);
    }

    public async Task NotifyPriceUpdateAsync(Guid auctionId, decimal newPrice)
    {
        await _hubContext.Clients.Group(auctionId.ToString()).ReceivePriceUpdate(newPrice);
    }

    public async Task NotifyAuctionEndedAsync(Guid auctionId, string winnerName, decimal finalPrice)
    {
        await _hubContext.Clients.Group(auctionId.ToString()).AuctionEnded(winnerName, finalPrice);
    }
}
