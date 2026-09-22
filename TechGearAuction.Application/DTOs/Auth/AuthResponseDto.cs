namespace TechGearAuction.Application.DTOs.Auth;

public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Token { get; set; } = null!;
}
