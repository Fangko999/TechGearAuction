using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Commands;

public class CreateAuctionCommand : IRequest<Guid>
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal StartPrice { get; set; }
    public decimal BidIncrement { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class CreateAuctionCommandHandler : IRequestHandler<CreateAuctionCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateAuctionCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
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

        var auction = new Auction
        {
            SellerId = _currentUserService.UserId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            StartPrice = request.StartPrice,
            CurrentPrice = request.StartPrice, // Current price initially equals start price
            BidIncrement = request.BidIncrement,
            BuyNowPrice = request.BuyNowPrice,
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime(),
            Status = AuctionStatus.Draft
        };

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync(cancellationToken);

        return auction.Id;
    }
}
