using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Commands;

public class PlaceBidCommand : IRequest
{
    public Guid AuctionId { get; set; }
    public decimal BidAmount { get; set; }
    public string IpAddress { get; set; } = null!;
    public string DeviceHash { get; set; } = null!;
}

public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuctionNotificationService _notificationService;

    public PlaceBidCommandHandler(IAppDbContext context, ICurrentUserService currentUserService, IAuctionNotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var bidderId = _currentUserService.UserId;

        // Note: RowVersion is used for Optimistic Concurrency Control (OCC) implicitly by EF Core
        var auction = await _context.Auctions
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
            throw new Exception("Auction not found.");

        if (auction.Status != AuctionStatus.Active)
            throw new Exception("This auction is not currently active.");

        if (auction.EndTime <= DateTime.UtcNow)
            throw new Exception("This auction has already ended.");

        if (auction.SellerId == bidderId)
            throw new Exception("You cannot bid on your own auction.");

        // Check if user is blocked by seller or vice versa
        var isBlocked = await _context.UserBlocks
            .AnyAsync(b => (b.BlockerId == auction.SellerId && b.BlockedId == bidderId) || 
                           (b.BlockerId == bidderId && b.BlockedId == auction.SellerId), cancellationToken);
        if (isBlocked)
        {
            throw new Exception("You are not allowed to bid on this auction.");
        }

        var minimumBid = auction.CurrentPrice + auction.BidIncrement;
        if (request.BidAmount < minimumBid)
        {
            throw new ArgumentException($"Bid amount must be at least {minimumBid}.");
        }

        if (auction.BuyNowPrice.HasValue && request.BidAmount >= auction.BuyNowPrice.Value)
        {
            throw new ArgumentException($"Bid amount cannot be greater than or equal to the Buy Now price ({auction.BuyNowPrice.Value}). Please use the Buy Now feature instead.");
        }

        var bidder = await _context.Users.FirstOrDefaultAsync(u => u.Id == bidderId, cancellationToken);
        var bidderName = bidder?.DisplayName ?? "Anonymous";

        // Create the bid
        var bid = new Bid
        {
            AuctionId = request.AuctionId,
            BidderId = bidderId,
            BidAmount = request.BidAmount,
            IpAddress = request.IpAddress,
            DeviceHash = request.DeviceHash
        };

        _context.Bids.Add(bid);

        // --- Anti-Spam Heuristic Logging (Module 5) ---
        var seller = await _context.Users.FirstOrDefaultAsync(u => u.Id == auction.SellerId, cancellationToken);
        bool matchSellerIp = !string.IsNullOrEmpty(request.IpAddress) && seller?.LastLoginIp == request.IpAddress;
        bool matchSellerDevice = !string.IsNullOrEmpty(request.DeviceHash) && seller?.LastLoginDeviceHash == request.DeviceHash;

        if (matchSellerIp || matchSellerDevice)
        {
            var matchField = matchSellerIp && matchSellerDevice ? "IP and Device" : (matchSellerIp ? "IP Address" : "Device Hash");
            _context.SuspiciousActivities.Add(new SuspiciousActivity
            {
                AuctionId = request.AuctionId,
                BidderId = bidderId,
                SellerId = auction.SellerId,
                IpAddress = request.IpAddress,
                DeviceHash = request.DeviceHash,
                Reason = $"Bidder {matchField} matches Seller."
            });
        }
        else
        {
            var matchingOtherBidder = await _context.Bids
                .Where(b => b.AuctionId == request.AuctionId && b.BidderId != bidderId && !b.IsCanceled &&
                            (b.IpAddress == request.IpAddress || b.DeviceHash == request.DeviceHash))
                .FirstOrDefaultAsync(cancellationToken);

            if (matchingOtherBidder != null)
            {
                var matchField = (matchingOtherBidder.IpAddress == request.IpAddress && matchingOtherBidder.DeviceHash == request.DeviceHash) 
                    ? "IP and Device" 
                    : (matchingOtherBidder.IpAddress == request.IpAddress ? "IP Address" : "Device Hash");

                _context.SuspiciousActivities.Add(new SuspiciousActivity
                {
                    AuctionId = request.AuctionId,
                    BidderId = bidderId,
                    SellerId = auction.SellerId,
                    IpAddress = request.IpAddress,
                    DeviceHash = request.DeviceHash,
                    Reason = $"Bidder {matchField} matches another Bidder ({matchingOtherBidder.BidderId})."
                });
            }
        }
        // ----------------------------------------------

        // Update auction price
        auction.CurrentPrice = request.BidAmount;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Catching concurrency conflict (Race Condition)
            throw new TechGearAuction.Application.Common.Exceptions.ConcurrencyException("Another user placed a bid at the exact same time. Please refresh and try again with a higher amount.");
        }

        // Notify via SignalR
        var bidTime = DateTime.UtcNow;
        await _notificationService.NotifyNewBidAsync(auction.Id, bidderName, bid.BidAmount, bidTime);
        await _notificationService.NotifyPriceUpdateAsync(auction.Id, auction.CurrentPrice);
    }
}

