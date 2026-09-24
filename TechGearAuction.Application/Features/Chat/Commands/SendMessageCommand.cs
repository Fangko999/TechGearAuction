using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Chat.Commands;

public class SendMessageCommand : IRequest<Guid>
{
    public Guid ChatRoomId { get; set; }
    public string Content { get; set; } = null!;
    public string MessageType { get; set; } = "Text";
    public string? MediaUrl { get; set; }
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IChatNotificationService _notificationService;

    public SendMessageCommandHandler(IAppDbContext context, ICurrentUserService currentUserService, IChatNotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task<Guid> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var chatRoom = await _context.ChatRooms
            .Include(r => r.Auction)
            .FirstOrDefaultAsync(r => r.Id == request.ChatRoomId, cancellationToken);

        if (chatRoom == null)
            throw new NotFoundException("Entity", "Chat room not found.");

        if (chatRoom.Status == ChatRoomStatus.Archived)
            throw new BusinessRuleException("This chat room is archived. You cannot send new messages.");

        if (chatRoom.Auction.SellerId != userId && chatRoom.Auction.WinnerId != userId)
            throw new BusinessRuleException("You are not a participant of this chat room.");

        var partnerId = chatRoom.Auction.SellerId == userId ? chatRoom.Auction.WinnerId!.Value : chatRoom.Auction.SellerId;

        // Check block status
        var isBlocked = await _context.UserBlocks
            .AnyAsync(b => (b.BlockerId == userId && b.BlockedId == partnerId) || 
                           (b.BlockerId == partnerId && b.BlockedId == userId), cancellationToken);

        if (isBlocked)
            throw new BusinessRuleException("You cannot send messages to this user because of a block.");

        var msgType = Enum.TryParse<ChatMessageType>(request.MessageType, out var parsedType) ? parsedType : ChatMessageType.Text;

        var message = new ChatMessage
        {
            ChatRoomId = request.ChatRoomId,
            SenderId = userId,
            Content = request.Content,
            MessageType = msgType,
            MediaUrl = request.MediaUrl,
            IsRead = false
        };

        // Auto-unarchive for the receiver
        if (chatRoom.Auction.SellerId == userId)
        {
            chatRoom.IsArchivedByWinner = false;
        }
        else
        {
            chatRoom.IsArchivedBySeller = false;
        }

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        // Notify
        await _notificationService.NotifyNewMessageAsync(request.ChatRoomId, userId, request.Content, request.MessageType, request.MediaUrl, message.CreatedAt);

        return message.Id;
    }
}

