namespace TechGearAuction.Domain.Entities;

public class AuctionWatch : BaseEntity
{
    public int UserId { get; set; }
    public int AuctionId { get; set; }

    public User User { get; set; } = null!;
    public Auction Auction { get; set; } = null!;
}