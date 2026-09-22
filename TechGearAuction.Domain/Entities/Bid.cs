namespace TechGearAuction.Domain.Entities;

public class Bid : BaseEntity
{
    public int AuctionId { get; set; }
    public int BidderId { get; set; }
    public decimal BidAmount { get; set; }
    public string IpAddress { get; set; } = null!;
    public string DeviceHash { get; set; } = null!;

    public Auction Auction { get; set; } = null!;
    public User Bidder { get; set; } = null!;
}