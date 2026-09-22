using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Commands;

public class UploadAuctionImageCommand : IRequest<string>
{
    public Guid AuctionId { get; set; }
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public class UploadAuctionImageCommandHandler : IRequestHandler<UploadAuctionImageCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStorageService _storageService;

    public UploadAuctionImageCommandHandler(IAppDbContext context, ICurrentUserService currentUserService, IStorageService storageService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _storageService = storageService;
    }

    public async Task<string> Handle(UploadAuctionImageCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
        {
            throw new Exception("Auction not found.");
        }

        if (auction.SellerId != _currentUserService.UserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to upload images for this auction.");
        }

        if (auction.Status != AuctionStatus.Draft && auction.Status != AuctionStatus.Scheduled)
        {
            throw new Exception("Cannot upload images to an active or completed auction.");
        }

        if (auction.Images.Count >= 10)
        {
            throw new Exception("Maximum of 10 images allowed per auction.");
        }

        var extension = Path.GetExtension(request.FileName);
        var uniqueFileName = $"{request.AuctionId}_{Guid.NewGuid()}{extension}";

        var url = await _storageService.UploadFileAsync(request.FileStream, uniqueFileName, request.ContentType, "auctions");

        var auctionImage = new AuctionImage
        {
            AuctionId = request.AuctionId,
            ImageUrl = url,
            IsPrimary = auction.Images.Count == 0 // Make the first image primary
        };

        _context.AuctionImages.Add(auctionImage);
        await _context.SaveChangesAsync(cancellationToken);

        return url;
    }
}
