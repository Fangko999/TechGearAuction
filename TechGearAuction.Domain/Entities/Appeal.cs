using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class Appeal : BaseEntity
{
    public Guid? ReportId { get; set; }
    public Guid UserId { get; set; }
    public string Description { get; set; } = null!;
    public AppealStatus Status { get; set; } = AppealStatus.Pending;

    public Report? Report { get; set; }
    public User User { get; set; } = null!;
    public ICollection<AppealEvidence> Evidences { get; set; } = new List<AppealEvidence>();
}
