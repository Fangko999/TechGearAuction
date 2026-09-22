namespace TechGearAuction.Domain.Entities;

public class ChatRoom : BaseEntity
{
    public Guid AuctionId { get; set; }

    public Auction Auction { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
