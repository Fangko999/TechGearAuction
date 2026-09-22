using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Commands;

public class UpdateDraftAuctionCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal StartPrice { get; set; }
    public decimal BidIncrement { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class UpdateDraftAuctionCommandHandler : IRequestHandler<UpdateDraftAuctionCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateDraftAuctionCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateDraftAuctionCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (auction == null)
        {
            throw new Exception("Auction not found.");
        }

        if (auction.SellerId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You can only edit your own auctions.");
        }

        if (auction.Status != AuctionStatus.Draft)
        {
            throw new Exception("Only draft auctions can be edited.");
        }

        if (request.StartPrice < 0) throw new ArgumentException("Start price cannot be negative.");
        if (request.BidIncrement <= 0) throw new ArgumentException("Bid increment must be greater than zero.");
        if (request.BuyNowPrice.HasValue && request.BuyNowPrice.Value < request.StartPrice)
        {
            throw new ArgumentException("Buy Now price must be greater than or equal to Start price.");
        }
        if (request.EndTime <= request.StartTime)
        {
            throw new ArgumentException("End time must be after Start time.");
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new Exception("Category does not exist.");
        }

        auction.CategoryId = request.CategoryId;
        auction.Title = request.Title;
        auction.Description = request.Description;
        auction.StartPrice = request.StartPrice;
        auction.CurrentPrice = request.StartPrice; // Reset current price as well
        auction.BidIncrement = request.BidIncrement;
        auction.BuyNowPrice = request.BuyNowPrice;
        auction.StartTime = request.StartTime.ToUniversalTime();
        auction.EndTime = request.EndTime.ToUniversalTime();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
