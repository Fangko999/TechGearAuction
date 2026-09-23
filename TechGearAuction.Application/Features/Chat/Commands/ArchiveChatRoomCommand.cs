using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Chat.Commands;

public class ArchiveChatRoomCommand : IRequest
{
    public Guid ChatRoomId { get; set; }
}

public class ArchiveChatRoomCommandHandler : IRequestHandler<ArchiveChatRoomCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ArchiveChatRoomCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ArchiveChatRoomCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var chatRoom = await _context.ChatRooms
            .Include(r => r.Auction)
            .FirstOrDefaultAsync(r => r.Id == request.ChatRoomId, cancellationToken);

        if (chatRoom == null)
            throw new Exception("Chat room not found.");

        if (chatRoom.Auction.SellerId == userId)
        {
            chatRoom.IsArchivedBySeller = true;
        }
        else if (chatRoom.Auction.WinnerId == userId)
        {
            chatRoom.IsArchivedByWinner = true;
        }
        else
        {
            throw new Exception("You are not a participant of this chat room.");
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
