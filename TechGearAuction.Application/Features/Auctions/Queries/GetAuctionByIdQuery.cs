using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Auction;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Auctions.Queries;

public class GetAuctionByIdQuery : IRequest<AuctionDetailDto>
{
    public Guid Id { get; set; }
}

public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, AuctionDetailDto>
{
    private readonly IAppDbContext _context;

    public GetAuctionByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<AuctionDetailDto> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions.AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Seller)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (auction == null)
        {
            throw new NotFoundException("Entity", "Auction not found.");
        }

        return new AuctionDetailDto
        {
            Id = auction.Id,
            CategoryId = auction.CategoryId,
            CategoryName = auction.Category.Name,
            Title = auction.Title,
            Description = auction.Description,
            StartPrice = auction.StartPrice,
            CurrentPrice = auction.CurrentPrice,
            BidIncrement = auction.BidIncrement,
            BuyNowPrice = auction.BuyNowPrice,
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            Status = auction.Status.ToString(),
            PrimaryImageUrl = auction.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl 
                              ?? auction.Images.FirstOrDefault()?.ImageUrl,
            SellerName = auction.Seller.DisplayName ?? auction.Seller.Email,
            
            // Seller info
            SellerId = auction.Seller.Id,
            SellerAvatar = auction.Seller.AvatarUrl,
            SellerAverageRating = auction.Seller.AverageRating,
            SellerTotalReviews = auction.Seller.TotalReviews,
            
            // Images
            Images = auction.Images.Select(img => new AuctionImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                IsPrimary = img.IsPrimary
            }).ToList()
        };
    }
}

