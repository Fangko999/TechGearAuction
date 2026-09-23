namespace TechGearAuction.Application.DTOs.Chat;

public class ChatRoomDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public string AuctionTitle { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public Guid PartnerId { get; set; }
    public string PartnerName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageDto
{
    public string Content { get; set; } = null!;
}
