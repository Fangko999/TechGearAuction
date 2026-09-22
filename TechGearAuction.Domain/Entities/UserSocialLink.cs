namespace TechGearAuction.Domain.Entities;

public class UserSocialLink : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Platform { get; set; }
    public string Url { get; set; } = null!;

    public User User { get; set; } = null!;
}
