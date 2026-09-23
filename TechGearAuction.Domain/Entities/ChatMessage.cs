using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid? SenderId { get; set; }
    public string Content { get; set; } = null!;
    public ChatMessageType MessageType { get; set; } = ChatMessageType.Text;
    public string? MediaUrl { get; set; }
    public bool IsRead { get; set; } = false;

    public ChatRoom ChatRoom { get; set; } = null!;
    public User? Sender { get; set; }
}
