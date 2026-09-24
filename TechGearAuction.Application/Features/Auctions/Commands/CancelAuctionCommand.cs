using TechGearAuction.Domain.Exceptions;
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
            throw new NotFoundException("Entity", "Auction not found.");
        }

        if (auction.SellerId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own auctions.");
        }

        if (auction.Status == AuctionStatus.Active)
        {
            throw new BusinessRuleException("Active auctions cannot be cancelled. There may be bidders participating.");
        }
        if (auction.Status != AuctionStatus.Draft && auction.Status != AuctionStatus.Scheduled)
        {
            throw new BusinessRuleException("Only draft or scheduled auctions can be cancelled.");
        }

        var oldStatus = auction.Status;
        auction.Status = AuctionStatus.Cancelled;

        // Refund the 1 credit if it was scheduled (since publishing cost 1 credit).
        if (oldStatus == AuctionStatus.Scheduled)
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

