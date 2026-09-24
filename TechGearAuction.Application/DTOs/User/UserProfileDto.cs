namespace TechGearAuction.Application.DTOs.User;

public record UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public int AvailableCredits { get; set; }
    public string Role { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ViolationCount { get; set; }
    public int ReportedAsSellerCount { get; set; }
    public int ReportedAsBuyerCount { get; set; }
    public int SuspiciousBidderCount { get; set; }
    public int SuspiciousSellerCount { get; set; }
    public List<SocialLinkDto> SocialLinks { get; set; } = new List<SocialLinkDto>();
}



