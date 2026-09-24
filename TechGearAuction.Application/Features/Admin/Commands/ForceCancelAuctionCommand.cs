using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Admin.Commands;

public class ForceCancelAuctionCommand : IRequest
{
    public Guid AuctionId { get; set; }
    public string Reason { get; set; } = null!;
}

public class ForceCancelAuctionCommandHandler : IRequestHandler<ForceCancelAuctionCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuctionNotificationService _notificationService;

    public ForceCancelAuctionCommandHandler(IAppDbContext context, ICurrentUserService currentUserService, IAuctionNotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task Handle(ForceCancelAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
            throw new NotFoundException("Entity", "Auction not found.");

        if (auction.Status == AuctionStatus.Completed || auction.Status == AuctionStatus.Cancelled)
            throw new BusinessRuleException("Auction is already ended or cancelled.");

        // Hoàn tiền nếu Scheduled/Active
        if (auction.Status == AuctionStatus.Scheduled || auction.Status == AuctionStatus.Active)
        {
            var seller = await _context.Users.FirstOrDefaultAsync(u => u.Id == auction.SellerId, cancellationToken);
            if (seller != null)
            {
                seller.AvailableCredits += 1;
                _context.CreditTransactions.Add(new CreditTransaction
                {
                    UserId = seller.Id,
                    Amount = 1,
                    Reason = $"Refund: Auction forcibly cancelled by Admin (ID: {auction.Id})",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        auction.Status = AuctionStatus.Cancelled;

        _context.AdminAuditLogs.Add(new AdminAuditLog
        {
            AdminId = _currentUserService.UserId,
            Action = "ForceCancelAuction",
            EntityType = "Auction",
            EntityId = auction.Id,
            Details = $"Admin forcibly cancelled auction. Reason: {request.Reason}"
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyAuctionEndedAsync(auction.Id, "No Winner (Cancelled by Admin)", auction.CurrentPrice);
    }
}
