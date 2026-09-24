namespace TechGearAuction.Application.DTOs.User;

public record UpdateProfileRequestDto
{
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<SocialLinkDto> SocialLinks { get; set; } = new List<SocialLinkDto>();
}

public record SocialLinkDto
{
    public string Platform { get; set; } = null!;
    public string Url { get; set; } = null!;
}


