using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Features.Users.Commands;
using TechGearAuction.Application.Features.Users.Queries;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me/watchlists/ending-soon")]
    public async Task<IActionResult> GetWatchlistEndingSoon()
    {
        var result = await _mediator.Send(new TechGearAuction.Application.Features.Auctions.Queries.GetWatchlistEndingSoonQuery());
        return Ok(result);
    }

    [HttpGet("me/bids/active")]
    public async Task<IActionResult> GetMyActiveBids()
    {
        var result = await _mediator.Send(new TechGearAuction.Application.Features.Users.Queries.GetMyActiveBidsQuery());
        return Ok(result);
    }

    [HttpGet("me/auctions/won")]
    public async Task<IActionResult> GetMyWonAuctions([FromQuery] TechGearAuction.Application.Features.Users.Queries.GetMyWonAuctionsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
            var result = await _mediator.Send(new GetMyProfileQuery());
            return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileCommand command)
    {
            await _mediator.Send(command);
            return Ok(new { Message = "Profile updated successfully." });
    }

    [HttpPut("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
            await _mediator.Send(command);
            return Ok(new { Message = "Password changed successfully." });
    }

    [HttpPost("me/avatar")]
    public async Task<IActionResult> UpdateAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Message = "File is missing." });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { Message = "File size cannot exceed 5MB." });
        }

        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { Message = "Only JPEG, PNG and WEBP images are allowed." });
        }

            using var stream = file.OpenReadStream();
            var command = new UpdateAvatarCommand
            {
                FileStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType
            };
            
            var url = await _mediator.Send(command);
            return Ok(new { Message = "Avatar updated successfully.", AvatarUrl = url });
    }

    [HttpGet("{id}/public-profile")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(Guid id)
    {
            var result = await _mediator.Send(new GetPublicProfileQuery { UserId = id });
            return Ok(result);
    }

    [HttpPost("me/credits/deposit")]
    public async Task<IActionResult> DepositCredit([FromBody] DepositCreditCommand command)
    {
            await _mediator.Send(command);
            return Ok(new { Message = "Credit deposited successfully." });
    }

    [HttpGet("me/credits/history")]
    public async Task<IActionResult> GetCreditHistory()
    {
            var result = await _mediator.Send(new GetMyCreditHistoryQuery());
            return Ok(result);
    }

    [HttpGet("me/watchlist")]
    public async Task<IActionResult> GetWatchlist([FromQuery] TechGearAuction.Application.Features.Auctions.Queries.GetMyWatchlistQuery query)
    {
            var result = await _mediator.Send(query);
            return Ok(result);
    }

    [HttpPost("{id}/follow")]
    public async Task<IActionResult> ToggleFollow(Guid id)
    {
            var isFollowing = await _mediator.Send(new TechGearAuction.Application.Features.Users.Commands.ToggleUserFollowCommand { FolloweeId = id });
            var status = isFollowing ? "followed" : "unfollowed";
            return Ok(new { Message = $"User {status} successfully.", IsFollowing = isFollowing });
    }

    [HttpGet("me/following")]
    public async Task<IActionResult> GetFollowing([FromQuery] TechGearAuction.Application.Features.Users.Queries.GetMyFollowingQuery query)
    {
            var result = await _mediator.Send(query);
            return Ok(result);
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> ToggleBlock(Guid id)
    {
            var isBlocked = await _mediator.Send(new TechGearAuction.Application.Features.Users.Commands.ToggleUserBlockCommand { BlockedId = id });
            var status = isBlocked ? "blocked" : "unblocked";
            return Ok(new { Message = $"User {status} successfully.", IsBlocked = isBlocked });
    }

    [HttpGet("me/blocked")]
    public async Task<IActionResult> GetBlockedUsers([FromQuery] TechGearAuction.Application.Features.Users.Queries.GetMyBlockedUsersQuery query)
    {
            var result = await _mediator.Send(query);
            return Ok(result);
    }
}

