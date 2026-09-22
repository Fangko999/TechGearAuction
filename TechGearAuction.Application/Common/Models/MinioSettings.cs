namespace TechGearAuction.Application.Common.Models;

public class MinioSettings
{
    public string Endpoint { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public bool UseSSL { get; set; }
    public MinioBuckets Buckets { get; set; } = new();
}

public class MinioBuckets
{
    public string Avatars { get; set; } = "avatars";
    public string Auctions { get; set; } = "auctions";
    public string Evidences { get; set; } = "evidences";
}

