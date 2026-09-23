using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Chat;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Chat.Queries;

public class GetMyChatRoomsQuery : IRequest<List<ChatRoomDto>>
{
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

        var rooms = await _context.ChatRooms
            .Include(r => r.Auction)
                .ThenInclude(a => a.Seller)
            .Include(r => r.Auction)
                .ThenInclude(a => a.Winner)
            .Where(r => r.Auction.SellerId == userId || r.Auction.WinnerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return rooms.Select(r => {
            var isSeller = r.Auction.SellerId == userId;
            var partner = isSeller ? r.Auction.Winner : r.Auction.Seller;

            return new ChatRoomDto
            {
                Id = r.Id,
                AuctionId = r.AuctionId,
                AuctionTitle = r.Auction.Title,
                Status = r.Status.ToString(),
                ExpiresAt = r.ExpiresAt,
                PartnerId = partner!.Id,
                PartnerName = partner.DisplayName,
                CreatedAt = r.CreatedAt
            };
        }).ToList();
    }
}
