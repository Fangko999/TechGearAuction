namespace TechGearAuction.Application.Hubs;

public interface IChatHub
{
    Task ReceiveNewMessage(Guid chatRoomId, Guid senderId, string content, DateTime sentAt);
    Task ReceiveMessageRead(Guid chatRoomId, Guid messageId);
}

