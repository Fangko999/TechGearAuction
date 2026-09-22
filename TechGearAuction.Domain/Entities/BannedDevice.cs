namespace TechGearAuction.Domain.Entities;

public class BannedDevice
{
    public string DeviceHash { get; set; } = null!;
    public string? Reason { get; set; }
    public DateTime BannedAt { get; set; }
}