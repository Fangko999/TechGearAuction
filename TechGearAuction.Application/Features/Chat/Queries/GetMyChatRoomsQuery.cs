using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Chat;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Chat.Queries;

public class GetMyChatRoomsQuery : IRequest<List<ChatRoomDto>>
{
    public string Role { get; set; } = "All"; // Buyer, Seller, All
    public string Folder { get; set; } = "Inbox"; // Inbox, Archived
}

public class GetMyChatRoomsQueryHandler : IRequestHandler<GetMyChatRoomsQuery, List<ChatRoomDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyChatRoomsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ChatRoomDto>> Handle(GetMyChatRoomsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var roleEnum = Enum.TryParse<ChatRoomRole>(request.Role, true, out var rEnum) ? rEnum : ChatRoomRole.All;
        var folderEnum = Enum.TryParse<ChatRoomFolder>(request.Folder, true, out var fEnum) ? fEnum : ChatRoomFolder.Inbox;

        var query = _context.ChatRooms.AsNoTracking()
            .Include(r => r.Auction)
                .ThenInclude(a => a.Seller)
            .Include(r => r.Auction)
                .ThenInclude(a => a.Winner)
            .Include(r => r.Auction)
                .ThenInclude(a => a.Images) // Load images to get thumbnail
            .Include(r => r.Messages) // Include messages for unread count
            .AsQueryable();

        // 1. Role Filter
        if (roleEnum == ChatRoomRole.Seller)
        {
            query = query.Where(r => r.Auction.SellerId == userId);
        }
        else if (roleEnum == ChatRoomRole.Buyer)
        {
            query = query.Where(r => r.Auction.WinnerId == userId);
        }
        else // All
        {
            query = query.Where(r => r.Auction.SellerId == userId || r.Auction.WinnerId == userId);
        }

        // 2. Folder Filter (Archive Flag)
        if (folderEnum == ChatRoomFolder.Archived)
        {
            query = query.Where(r => (r.Auction.SellerId == userId && r.IsArchivedBySeller) ||
                                     (r.Auction.WinnerId == userId && r.IsArchivedByWinner));
        }
        else // Inbox
        {
            query = query.Where(r => (r.Auction.SellerId == userId && !r.IsArchivedBySeller) ||
                                     (r.Auction.WinnerId == userId && !r.IsArchivedByWinner));
        }

        var rooms = await query
            .OrderByDescending(r => r.CreatedAt) // Will sort by Latest Message in a real app, simplified here
            .ToListAsync(cancellationToken);

        return rooms.Select(r => {
            var isSeller = r.Auction.SellerId == userId;
            var opponent = isSeller ? r.Auction.Winner : r.Auction.Seller;
            var thumbnail = r.Auction.Images.OrderByDescending(img => img.IsPrimary).FirstOrDefault()?.ImageUrl ?? "";
            
            var unreadCount = r.Messages.Count(m => !m.IsRead && m.SenderId != userId && m.SenderId != null);

            return new ChatRoomDto
            {
                Id = r.Id,
                AuctionId = r.AuctionId,
                AuctionTitle = r.Auction.Title,
                Status = r.Status.ToString(),
                ExpiresAt = r.ExpiresAt,
                AuctionThumbnailUrl = thumbnail,
                OpponentId = opponent!.Id,
                OpponentName = opponent.DisplayName ?? string.Empty,
                OpponentAvatarUrl = opponent.AvatarUrl,
                UnreadCount = unreadCount,
                CreatedAt = r.CreatedAt
            };
        }).ToList();
    }
}
