using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Users.Commands;

public class ToggleUserBlockCommand : IRequest<bool>
{
    public Guid BlockedId { get; set; }
}

public class ToggleUserBlockCommandHandler : IRequestHandler<ToggleUserBlockCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuctionNotificationService _notificationService;

    public ToggleUserBlockCommandHandler(IAppDbContext context, ICurrentUserService currentUserService, IAuctionNotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task<bool> Handle(ToggleUserBlockCommand request, CancellationToken cancellationToken)
    {
        var blockerId = _currentUserService.UserId;

        if (blockerId == request.BlockedId)
        {
            throw new ArgumentException("You cannot block yourself.");
        }

        var blockedUserExists = await _context.Users.AnyAsync(u => u.Id == request.BlockedId, cancellationToken);
        if (!blockedUserExists)
        {
            throw new NotFoundException("Entity", "User not found.");
        }

        var existingBlock = await _context.UserBlocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == request.BlockedId, cancellationToken);

        if (existingBlock != null)
        {
            // Unblock
            _context.UserBlocks.Remove(existingBlock);
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
        else
        {
            // Block
            var block = new UserBlock
            {
                BlockerId = blockerId,
                BlockedId = request.BlockedId
            };
            _context.UserBlocks.Add(block);

            // Break any existing follows in BOTH directions
            var followsToBreak = await _context.UserFollows
                .Where(f => (f.FollowerId == blockerId && f.FolloweeId == request.BlockedId) ||
                            (f.FollowerId == request.BlockedId && f.FolloweeId == blockerId))
                .ToListAsync(cancellationToken);

            if (followsToBreak.Any())
            {
                _context.UserFollows.RemoveRange(followsToBreak);
            }

            // Rollback Price if the blocked user is currently Top Bidder in any of the blocker's Active auctions
            var activeAuctions = await _context.Auctions
                .Where(a => a.SellerId == blockerId && a.Status == Domain.Enums.AuctionStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var auction in activeAuctions)
            {
                var topBid = await _context.Bids
                    .Where(b => b.AuctionId == auction.Id && !b.IsCanceled)
                    .OrderByDescending(b => b.BidAmount)
                    .FirstOrDefaultAsync(cancellationToken);

                if (topBid != null && topBid.BidderId == request.BlockedId)
                {
                    // Cancel ALL bids of this user on this auction to prevent them from falling back to their 2nd bid
                    var userBids = await _context.Bids
                        .Where(b => b.AuctionId == auction.Id && b.BidderId == request.BlockedId && !b.IsCanceled)
                        .ToListAsync(cancellationToken);

                    foreach (var bid in userBids)
                    {
                        bid.IsCanceled = true;
                    }

                    // Calculate new price from DB by explicitly ignoring the blocked user's bids
                    var highestValidBid = await _context.Bids
                        .Where(b => b.AuctionId == auction.Id && b.BidderId != request.BlockedId && !b.IsCanceled)
                        .OrderByDescending(b => b.BidAmount)
                        .FirstOrDefaultAsync(cancellationToken);

                    var newPrice = highestValidBid?.BidAmount ?? auction.StartPrice;
                    auction.CurrentPrice = newPrice;

                    await _notificationService.NotifyPriceUpdateAsync(auction.Id, newPrice);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
