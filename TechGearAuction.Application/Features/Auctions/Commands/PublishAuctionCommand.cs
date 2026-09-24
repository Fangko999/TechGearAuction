using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Commands;

public class PublishAuctionCommand : IRequest
{
    public Guid AuctionId { get; set; }
}

public class PublishAuctionCommandHandler : IRequestHandler<PublishAuctionCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public PublishAuctionCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(PublishAuctionCommand request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUserService.UserId;
        
        var auction = await _context.Auctions
            .Include(a => a.Images)
            .Include(a => a.Seller)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
        {
            throw new NotFoundException("Entity", "Auction not found.");
        }

        if (auction.SellerId != sellerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to publish this auction.");
        }

        if (auction.Status != AuctionStatus.Draft)
        {
            throw new BusinessRuleException("Only Draft auctions can be published.");
        }

        if (!auction.Images.Any())
        {
            throw new BusinessRuleException("Auction must have at least one image before publishing.");
        }

        if (auction.Seller.AvailableCredits < 1)
        {
            throw new BusinessRuleException("Not enough credits to publish this auction. Please deposit credits.");
        }

        // Deduct credit
        auction.Seller.AvailableCredits -= 1;
        auction.Seller.TotalAuctionsCreated += 1; // Update stats

        var transaction = new CreditTransaction
        {
            UserId = sellerId,
            Amount = -1,
            Reason = "Phí đăng phiên đấu giá",
            AuctionId = auction.Id
        };
        _context.CreditTransactions.Add(transaction);

        // Update status based on start time
        if (auction.StartTime <= DateTime.UtcNow)
        {
            auction.Status = AuctionStatus.Active;
        }
        else
        {
            auction.Status = AuctionStatus.Scheduled;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

