namespace TechGearAuction.Application.Interfaces;

public interface IAuctionNotificationService
{
    Task NotifyNewBidAsync(Guid auctionId, string bidderName, decimal amount, DateTime time);
    Task NotifyPriceUpdateAsync(Guid auctionId, decimal newPrice);
    Task NotifyAuctionEndedAsync(Guid auctionId, string winnerName, decimal finalPrice);
}

