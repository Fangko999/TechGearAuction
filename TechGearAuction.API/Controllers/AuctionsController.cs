using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.Auctions.Commands;
using TechGearAuction.Application.Features.Auctions.Queries;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuctionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuctionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionCommand command)
    {
            var id = await _mediator.Send(command);
            return Ok(new { Message = "Auction created as draft successfully.", Id = id });
    }

    [HttpPost("{id}/images")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Message = "Invalid file." });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { Message = "File size exceeds 5MB limit." });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { Message = "Invalid file format. Only JPG, PNG, and WebP are allowed." });
        }

            using var stream = file.OpenReadStream();
            var command = new UploadAuctionImageCommand
            {
                AuctionId = id,
                FileStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType
            };
            
            var url = await _mediator.Send(command);
            return Ok(new { Message = "Image uploaded successfully.", ImageUrl = url });
    }

    [HttpPut("{id}/publish")]
    public async Task<IActionResult> PublishAuction(Guid id)
    {
            await _mediator.Send(new PublishAuctionCommand { AuctionId = id });
            return Ok(new { Message = "Auction published successfully. 1 credit was deducted." });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctions([FromQuery] GetAuctionsQuery query)
    {
            var result = await _mediator.Send(query);
            return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
            var result = await _mediator.Send(new GetAuctionByIdQuery { Id = id });
            return Ok(result);
    }

    [HttpPost("{id}/watch")]
    public async Task<IActionResult> ToggleWatch(Guid id)
    {
            var isWatched = await _mediator.Send(new ToggleAuctionWatchCommand { AuctionId = id });
            var status = isWatched ? "added to" : "removed from";
            return Ok(new { Message = $"Auction {status} watchlist successfully.", IsWatched = isWatched });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDraftAuction(Guid id, [FromBody] UpdateDraftAuctionCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { Message = "ID mismatch." });
        }

            await _mediator.Send(command);
            return Ok(new { Message = "Auction updated successfully." });
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelAuction(Guid id)
    {
            await _mediator.Send(new CancelAuctionCommand { Id = id });
            return Ok(new { Message = "Auction cancelled successfully." });
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTrendingAuctions()
    {
        var result = await _mediator.Send(new GetTrendingAuctionsQuery());
        return Ok(result);
    }

    [HttpGet("ending-soon")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEndingSoonAuctions()
    {
        var result = await _mediator.Send(new GetEndingSoonAuctionsQuery());
        return Ok(result);
    }

    [HttpGet("recent-winners")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRecentWinners()
    {
        var result = await _mediator.Send(new GetRecentWinnersQuery());
        return Ok(result);
    }

    [HttpGet("feed")]
    [Authorize]
    public async Task<IActionResult> GetFeedAuctions([FromQuery] GetFeedAuctionsQuery query)
    {
            var result = await _mediator.Send(query);
            return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAuctions([FromQuery] GetMyAuctionsQuery query)
    {
            var result = await _mediator.Send(query);
            return Ok(result);
    }

    [HttpPost("{id}/bids")]
    public async Task<IActionResult> PlaceBid(Guid id, [FromBody] TechGearAuction.Application.DTOs.Auction.PlaceBidDto dto)
    {
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                ?? HttpContext.Connection.RemoteIpAddress?.ToString() 
                ?? "Unknown";
            
            var deviceHash = Request.Headers["X-Device-Hash"].FirstOrDefault() 
                ?? "Unknown";

            var command = new PlaceBidCommand
            {
                AuctionId = id,
                BidAmount = dto.BidAmount,
                IpAddress = ipAddress,
                DeviceHash = deviceHash
            };

            await _mediator.Send(command);
            return Ok(new { Message = "Bid placed successfully." });
    }

    [HttpPost("{id}/buy-now")]
    [Authorize]
    public async Task<IActionResult> BuyNow(Guid id)
    {
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                ?? HttpContext.Connection.RemoteIpAddress?.ToString() 
                ?? "Unknown";
            
            var deviceHash = Request.Headers["X-Device-Hash"].FirstOrDefault() 
                ?? "Unknown";

            var command = new BuyNowCommand
            {
                AuctionId = id,
                IpAddress = ipAddress,
                DeviceHash = deviceHash
            };

            await _mediator.Send(command);
            return Ok(new { Message = "Buy Now successful." });
    }
}

