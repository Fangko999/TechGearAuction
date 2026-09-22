namespace TechGearAuction.Domain.Entities;

public class UserBlock : BaseEntity
{
    public Guid BlockerId { get; set; }
    public Guid BlockedId { get; set; }

    public User Blocker { get; set; } = null!;
    public User Blocked { get; set; } = null!;
}
