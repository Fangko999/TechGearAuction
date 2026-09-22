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
        try
        {
            var id = await _mediator.Send(command);
            return Ok(new { Message = "Auction created as draft successfully.", Id = id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
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

        try
        {
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
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}/publish")]
    public async Task<IActionResult> PublishAuction(Guid id)
    {
        try
        {
            await _mediator.Send(new PublishAuctionCommand { AuctionId = id });
            return Ok(new { Message = "Auction published successfully. 1 credit was deducted." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctions([FromQuery] GetAuctionsQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetAuctionByIdQuery { Id = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/watch")]
    public async Task<IActionResult> ToggleWatch(Guid id)
    {
        try
        {
            var isWatched = await _mediator.Send(new ToggleAuctionWatchCommand { AuctionId = id });
            var status = isWatched ? "added to" : "removed from";
            return Ok(new { Message = $"Auction {status} watchlist successfully.", IsWatched = isWatched });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDraftAuction(Guid id, [FromBody] UpdateDraftAuctionCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { Message = "ID mismatch." });
        }

        try
        {
            await _mediator.Send(command);
            return Ok(new { Message = "Auction updated successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelAuction(Guid id)
    {
        try
        {
            await _mediator.Send(new CancelAuctionCommand { Id = id });
            return Ok(new { Message = "Auction cancelled successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAuctions([FromQuery] GetMyAuctionsQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

