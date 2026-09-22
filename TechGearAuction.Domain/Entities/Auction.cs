using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class Auction : BaseEntity
{
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal StartPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal BidIncrement { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AuctionStatus Status { get; set; } = AuctionStatus.Draft;
    public Guid? WinnerId { get; set; }
    public byte[]? RowVersion { get; set; }

    public User Seller { get; set; } = null!;
    public User? Winner { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<AuctionImage> Images { get; set; } = new List<AuctionImage>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ChatRoom? ChatRoom { get; set; }
}
