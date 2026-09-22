namespace TechGearAuction.Domain.Entities;

public class AdminAuditLog : BaseEntity
{
    public Guid AdminId { get; set; }
    public User Admin { get; set; } = null!;
    
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string? Details { get; set; }
}


