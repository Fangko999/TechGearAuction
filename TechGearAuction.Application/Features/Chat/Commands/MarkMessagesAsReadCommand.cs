using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Chat.Commands;

public class MarkMessagesAsReadCommand : IRequest
{
    public Guid ChatRoomId { get; set; }
}

public class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IChatNotificationService _notificationService;

    public MarkMessagesAsReadCommandHandler(IAppDbContext context, ICurrentUserService currentUserService, IChatNotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var chatRoom = await _context.ChatRooms
            .Include(r => r.Auction)
            .FirstOrDefaultAsync(r => r.Id == request.ChatRoomId, cancellationToken);

        if (chatRoom == null)
            throw new Exception("Chat room not found.");

        if (chatRoom.Auction.SellerId != userId && chatRoom.Auction.WinnerId != userId)
            throw new Exception("You are not a participant of this chat room.");

        var unreadMessages = await _context.ChatMessages
            .Where(m => m.ChatRoomId == request.ChatRoomId && m.SenderId != userId && !m.IsRead)
            .ToListAsync(cancellationToken);

        if (unreadMessages.Any())
        {
            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }
            await _context.SaveChangesAsync(cancellationToken);

            // Notify partner that messages were read. For simplicity, we can notify each or just trigger one event.
            // In a real system, you might pass a list of message IDs.
            foreach (var msg in unreadMessages)
            {
                await _notificationService.NotifyMessageReadAsync(request.ChatRoomId, msg.Id);
            }
        }
    }
}
