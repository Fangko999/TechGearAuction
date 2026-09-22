namespace TechGearAuction.Domain.Entities;

public class SuspiciousActivity : BaseEntity
{
    public Guid? AuctionId { get; set; }
    public Guid? BidderId { get; set; }
    public Guid? SellerId { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceHash { get; set; }
    public string? Reason { get; set; }
    public bool IsReviewed { get; set; } = false;

    public Auction? Auction { get; set; }
    public User? Bidder { get; set; }
    public User? Seller { get; set; }
}
