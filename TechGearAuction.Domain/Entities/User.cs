using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public int AvailableCredits { get; set; } = 3;
    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public string? LastLoginIp { get; set; }
    public string? LastLoginDeviceHash { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Rating / Reviews
    public double AverageRating { get; set; } = 0;
    public int TotalReviews { get; set; } = 0;

    // Transactions & Reports
    public int TotalAuctionsCreated { get; set; } = 0;
    public int TotalAuctionsWon { get; set; } = 0;
    public int ReportedAsSellerCount { get; set; } = 0;
    public int ReportedAsBuyerCount { get; set; } = 0;
    public int SuspiciousBidderCount { get; set; } = 0;
    public int SuspiciousSellerCount { get; set; } = 0;

    public ICollection<UserSocialLink> SocialLinks { get; set; } = new List<UserSocialLink>();
}