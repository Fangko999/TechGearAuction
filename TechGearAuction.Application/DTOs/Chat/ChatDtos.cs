namespace TechGearAuction.Application.DTOs.Chat;

public class ChatRoomDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public string AuctionTitle { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string AuctionThumbnailUrl { get; set; } = null!;
    public Guid OpponentId { get; set; }
    public string OpponentName { get; set; } = null!;
    public string? OpponentAvatarUrl { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid ChatRoomId { get; set; }
    public Guid? SenderId { get; set; }
    public string? SenderName { get; set; }
    public string Content { get; set; } = null!;
    public string MessageType { get; set; } = null!;
    public string? MediaUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageDto
{
    public string Content { get; set; } = null!;
    public string MessageType { get; set; } = "Text";
    public string? MediaUrl { get; set; }
}

