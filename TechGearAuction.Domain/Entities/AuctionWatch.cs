namespace TechGearAuction.Domain.Entities;

public class AuctionWatch : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AuctionId { get; set; }

    public User User { get; set; } = null!;
    public Auction Auction { get; set; } = null!;
}
