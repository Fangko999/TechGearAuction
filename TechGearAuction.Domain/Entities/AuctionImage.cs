namespace TechGearAuction.Domain.Entities;

public class AuctionImage : BaseEntity
{
    public int AuctionId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;

    public Auction Auction { get; set; } = null!;
}