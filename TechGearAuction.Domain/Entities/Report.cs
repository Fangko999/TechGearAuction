using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class Report : BaseEntity
{
    public Guid ReporterId { get; set; }
    public Guid ReportedUserId { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ChatRoomId { get; set; }
    public ReportType Type { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public User Reporter { get; set; } = null!;
    public User ReportedUser { get; set; } = null!;
    public Auction? Auction { get; set; }
    public Appeal? Appeal { get; set; }
    public ICollection<ReportEvidence> Evidences { get; set; } = new List<ReportEvidence>();
}
