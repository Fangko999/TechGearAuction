namespace TechGearAuction.Application.DTOs.User;

public class UpdateProfileRequestDto
{
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public List<SocialLinkDto> SocialLinks { get; set; } = new List<SocialLinkDto>();
}

public class SocialLinkDto
{
    public string Platform { get; set; } = null!;
    public string Url { get; set; } = null!;
}
