using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class ReportEvidence : BaseEntity
{
    public Guid ReportId { get; set; }
    public string MediaUrl { get; set; } = null!;
    public MediaType Type { get; set; } = MediaType.Image;

    public Report Report { get; set; } = null!;
}
