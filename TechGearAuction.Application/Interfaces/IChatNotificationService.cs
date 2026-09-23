namespace TechGearAuction.Application.Interfaces;

public interface IChatNotificationService
{
    Task NotifyNewMessageAsync(Guid chatRoomId, Guid senderId, string content, DateTime sentAt);
    Task NotifyMessageReadAsync(Guid chatRoomId, Guid messageId);
}
