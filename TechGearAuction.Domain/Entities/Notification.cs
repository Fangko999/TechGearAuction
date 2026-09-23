using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; } = false;
    public string? Link { get; set; }
    public User User { get; set; } = null!;
}

