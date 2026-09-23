namespace TechGearAuction.Application.Hubs;

public interface IAuctionHub
{
    Task ReceiveNewBid(string bidderName, decimal amount, DateTime time);
    Task ReceivePriceUpdate(decimal newPrice);
    Task AuctionEnded(string winnerName, decimal finalPrice);
}

