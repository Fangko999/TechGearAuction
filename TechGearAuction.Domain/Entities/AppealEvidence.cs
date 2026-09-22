using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class AppealEvidence : BaseEntity
{
    public Guid AppealId { get; set; }
    public string MediaUrl { get; set; } = null!;
    public MediaType Type { get; set; } = MediaType.Image;

    public Appeal Appeal { get; set; } = null!;
}
