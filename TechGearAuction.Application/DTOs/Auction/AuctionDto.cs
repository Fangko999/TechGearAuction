namespace TechGearAuction.Application.DTOs.Auction;

public class AuctionImageDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

public class AuctionDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public decimal StartPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
    public string? PrimaryImageUrl { get; set; }
    public string SellerName { get; set; } = null!;
}

public class AuctionDetailDto : AuctionDto
{
    public string? Description { get; set; }
    public decimal BidIncrement { get; set; }
    
    // Seller Trust info
    public Guid SellerId { get; set; }
    public string? SellerAvatar { get; set; }
    public double SellerAverageRating { get; set; }
    public int SellerTotalReviews { get; set; }

    public List<AuctionImageDto> Images { get; set; } = new List<AuctionImageDto>();
}
