namespace TechGearAuction.Domain.Entities;

public class UserDeviceLog : BaseEntity
{
    public Guid UserId { get; set; }
    public string IpAddress { get; set; } = null!;
    public string DeviceHash { get; set; } = null!;
    
    // Using BaseEntity.CreatedAt as LoginTime, or explicit:
    public DateTime LoginTime { get; set; }
}

