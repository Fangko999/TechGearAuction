using Microsoft.AspNetCore.SignalR;
using TechGearAuction.API.Hubs;
using TechGearAuction.Application.Hubs;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.API.Services;

public class ChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub, IChatHub> _hubContext;

    public ChatNotificationService(IHubContext<ChatHub, IChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewMessageAsync(Guid chatRoomId, Guid senderId, string content, DateTime sentAt)
    {
        await _hubContext.Clients.Group(chatRoomId.ToString()).ReceiveNewMessage(chatRoomId, senderId, content, sentAt);
    }

    public async Task NotifyMessageReadAsync(Guid chatRoomId, Guid messageId)
    {
        await _hubContext.Clients.Group(chatRoomId.ToString()).ReceiveMessageRead(chatRoomId, messageId);
    }
}
