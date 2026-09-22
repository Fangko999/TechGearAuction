using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Commands;

public class CancelAuctionCommand : IRequest
{
    public Guid Id { get; set; }
}

public class CancelAuctionCommandHandler : IRequestHandler<CancelAuctionCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CancelAuctionCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CancelAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (auction == null)
        {
            throw new Exception("Auction not found.");
        }

        if (auction.SellerId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own auctions.");
        }

        if (auction.Status != AuctionStatus.Draft && auction.Status != AuctionStatus.Scheduled)
        {
            throw new Exception("Only draft or scheduled auctions can be cancelled. Active auctions cannot be cancelled directly.");
        }

        auction.Status = AuctionStatus.Cancelled;

        // Optionally, we could refund the 1 credit here if it was scheduled (since publishing cost 1 credit).
        if (auction.Status == AuctionStatus.Scheduled)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);
            if (user != null)
            {
                user.AvailableCredits += 1;
                _context.CreditTransactions.Add(new Domain.Entities.CreditTransaction
                {
                    UserId = user.Id,
                    Amount = 1,
                    Reason = "Refund: Auction Cancelled",
                    AuctionId = auction.Id
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

