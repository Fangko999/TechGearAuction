namespace TechGearAuction.Domain.Entities;

public class Bid : BaseEntity
{
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }
    public decimal BidAmount { get; set; }
    public string IpAddress { get; set; } = null!;
    public string DeviceHash { get; set; } = null!;
    public bool IsCanceled { get; set; } = false;

    public Auction Auction { get; set; } = null!;
    public User Bidder { get; set; } = null!;
}
