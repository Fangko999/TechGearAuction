using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class ChatRoom : BaseEntity
{
    public Guid AuctionId { get; set; }
    public ChatRoomStatus Status { get; set; } = ChatRoomStatus.Active;
    public DateTime ExpiresAt { get; set; }
    public bool IsArchivedBySeller { get; set; } = false;
    public bool IsArchivedByWinner { get; set; } = false;

    public Auction Auction { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
