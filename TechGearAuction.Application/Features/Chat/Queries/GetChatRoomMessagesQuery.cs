using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.DTOs.Chat;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Chat.Queries;

public class GetChatRoomMessagesQuery : IRequest<PagedResult<ChatMessageDto>>
{
    public Guid ChatRoomId { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class GetChatRoomMessagesQueryHandler : IRequestHandler<GetChatRoomMessagesQuery, PagedResult<ChatMessageDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetChatRoomMessagesQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<ChatMessageDto>> Handle(GetChatRoomMessagesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var chatRoom = await _context.ChatRooms
            .Include(r => r.Auction)
            .FirstOrDefaultAsync(r => r.Id == request.ChatRoomId, cancellationToken);

        if (chatRoom == null)
            throw new Exception("Chat room not found.");

        if (chatRoom.Auction.SellerId != userId && chatRoom.Auction.WinnerId != userId)
            throw new Exception("You are not a participant of this chat room.");

        var query = _context.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.ChatRoomId == request.ChatRoomId)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var messages = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Return in chronological order for the client
        var items = messages.OrderBy(m => m.CreatedAt).Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ChatRoomId = m.ChatRoomId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.DisplayName,
            Content = m.Content,
            MessageType = m.MessageType.ToString(),
            MediaUrl = m.MediaUrl,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        }).ToList();

        return new PagedResult<ChatMessageDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}
