using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Domain.Exceptions;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Commands;

public class BuyNowCommand : IRequest
{
    public Guid AuctionId { get; set; }
    public string IpAddress { get; set; } = null!;
    public string DeviceHash { get; set; } = null!;
}

public class BuyNowCommandHandler : IRequestHandler<BuyNowCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuctionNotificationService _notificationService;

    public BuyNowCommandHandler(IAppDbContext context, ICurrentUserService currentUserService, IAuctionNotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task Handle(BuyNowCommand request, CancellationToken cancellationToken)
    {
        var bidderId = _currentUserService.UserId;

        var auction = await _context.Auctions
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
            throw new NotFoundException("Entity", "Auction not found.");

        if (auction.Status != AuctionStatus.Active)
            throw new BusinessRuleException("This auction is not currently active.");

        if (auction.EndTime <= DateTime.UtcNow)
            throw new BusinessRuleException("This auction has already ended.");

        if (auction.SellerId == bidderId)
            throw new BusinessRuleException("You cannot buy your own auction.");

        if (!auction.BuyNowPrice.HasValue)
            throw new BusinessRuleException("This auction does not have a Buy Now option.");

        var isBlocked = await _context.UserBlocks
            .AnyAsync(b => (b.BlockerId == auction.SellerId && b.BlockedId == bidderId) || 
                           (b.BlockerId == bidderId && b.BlockedId == auction.SellerId), cancellationToken);
        if (isBlocked)
        {
            throw new ForbiddenException("You are not allowed to buy this auction.");
        }

        var bidder = await _context.Users.FirstOrDefaultAsync(u => u.Id == bidderId, cancellationToken);
        var bidderName = bidder?.DisplayName ?? "Anonymous";

        // Create the bid at BuyNow price
        var bid = new Bid
        {
            AuctionId = request.AuctionId,
            BidderId = bidderId,
            BidAmount = auction.BuyNowPrice.Value,
            IpAddress = request.IpAddress,
            DeviceHash = request.DeviceHash
        };

        _context.Bids.Add(bid);

        // Update auction
        auction.CurrentPrice = auction.BuyNowPrice.Value;
        auction.Status = AuctionStatus.Completed;
        auction.WinnerId = bidderId;

        // Create ChatRoom
        var chatRoom = new ChatRoom
        {
            AuctionId = auction.Id,
            Status = ChatRoomStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        _context.ChatRooms.Add(chatRoom);

        var sysMessage = new ChatMessage
        {
            ChatRoom = chatRoom,
            SenderId = null,
            Content = $"Phòng chat tự động khởi tạo. Giá chốt: {auction.CurrentPrice:N0}",
            MessageType = ChatMessageType.SystemText,
            IsRead = false
        };
        _context.ChatMessages.Add(sysMessage);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException("Another user modified this auction at the exact same time. Please try again.");
        }

        // Notify via SignalR
        await _notificationService.NotifyAuctionEndedAsync(auction.Id, bidderName, auction.CurrentPrice);
    }
}


