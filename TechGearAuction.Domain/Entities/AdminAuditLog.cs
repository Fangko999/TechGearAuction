namespace TechGearAuction.Domain.Entities;

public class AdminAuditLog : BaseEntity
{
    public int AdminId { get; set; }
    public User Admin { get; set; } = null!;
    
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public int EntityId { get; set; }
    public string? Details { get; set; }
}

