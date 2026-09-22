namespace TechGearAuction.Application.DTOs.User;

public class PublicProfileDto
{
    public Guid Id { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Rating / Reviews
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Transactions & Reports
    public int TotalAuctionsCreated { get; set; }
    public int TotalAuctionsWon { get; set; }
    public int ReportedAsSellerCount { get; set; }
    public int ReportedAsBuyerCount { get; set; }

    public List<SocialLinkDto> SocialLinks { get; set; } = new List<SocialLinkDto>();
}
