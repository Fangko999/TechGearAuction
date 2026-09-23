using Microsoft.AspNetCore.SignalR;
using TechGearAuction.Application.Hubs;

namespace TechGearAuction.API.Hubs;

public class ChatHub : Hub<IChatHub>
{
    public async Task JoinChatRoom(Guid chatRoomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId.ToString());
    }

    public async Task LeaveChatRoom(Guid chatRoomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId.ToString());
    }
}
