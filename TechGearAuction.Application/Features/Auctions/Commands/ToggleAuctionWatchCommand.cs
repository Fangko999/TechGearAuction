using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Auctions.Commands;

public class ToggleAuctionWatchCommand : IRequest<bool>
{
    public Guid AuctionId { get; set; }
}

public class ToggleAuctionWatchCommandHandler : IRequestHandler<ToggleAuctionWatchCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ToggleAuctionWatchCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ToggleAuctionWatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var auctionExists = await _context.Auctions.AnyAsync(a => a.Id == request.AuctionId, cancellationToken);
        if (!auctionExists)
        {
            throw new Exception("Auction not found.");
        }

        var existingWatch = await _context.AuctionWatches
            .FirstOrDefaultAsync(w => w.UserId == userId && w.AuctionId == request.AuctionId, cancellationToken);

        if (existingWatch != null)
        {
            // Unwatch (Soft delete will be applied automatically by SaveChangesAsync)
            _context.AuctionWatches.Remove(existingWatch);
            await _context.SaveChangesAsync(cancellationToken);
            return false; // Returns false indicating it's now unwatched
        }
        else
        {
            // Watch
            var watch = new AuctionWatch
            {
                UserId = userId,
                AuctionId = request.AuctionId
            };
            _context.AuctionWatches.Add(watch);
            await _context.SaveChangesAsync(cancellationToken);
            return true; // Returns true indicating it's now watched
        }
    }
}

