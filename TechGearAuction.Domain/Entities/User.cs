using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsEmailVerified { get; set; } = false;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public int AvailableCredits { get; set; } = 3;
    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public string? LastLoginIp { get; set; }
    public string? LastLoginDeviceHash { get; set; }

    public ICollection<UserSocialLink> SocialLinks { get; set; } = new List<UserSocialLink>();
}