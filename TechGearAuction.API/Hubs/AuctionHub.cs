using Microsoft.AspNetCore.SignalR;
using TechGearAuction.Application.Hubs;

namespace TechGearAuction.API.Hubs;

public class AuctionHub : Hub<IAuctionHub>
{
    public async Task JoinAuctionGroup(Guid auctionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());
    }

    public async Task LeaveAuctionGroup(Guid auctionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
    }
}

