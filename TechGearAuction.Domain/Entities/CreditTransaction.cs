namespace TechGearAuction.Domain.Entities;

public class CreditTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public int Amount { get; set; }
    public string? Reason { get; set; }
    public Guid? AuctionId { get; set; }

    public User User { get; set; } = null!;
    public Auction? Auction { get; set; }
}
