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

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var result = await _mediator.Send(new GetMyProfileQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { Message = "Profile updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { Message = "Password changed successfully." });
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

        try
        {
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
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}/public-profile")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetPublicProfileQuery { UserId = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPost("me/credits/deposit")]
    public async Task<IActionResult> DepositCredit([FromBody] DepositCreditCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { Message = "Credit deposited successfully." });
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

    [HttpGet("me/credits/history")]
    public async Task<IActionResult> GetCreditHistory()
    {
        try
        {
            var result = await _mediator.Send(new GetMyCreditHistoryQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("me/watchlist")]
    public async Task<IActionResult> GetWatchlist([FromQuery] TechGearAuction.Application.Features.Auctions.Queries.GetMyWatchlistQuery query)
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

    [HttpPost("{id}/follow")]
    public async Task<IActionResult> ToggleFollow(Guid id)
    {
        try
        {
            var isFollowing = await _mediator.Send(new TechGearAuction.Application.Features.Users.Commands.ToggleUserFollowCommand { FolloweeId = id });
            var status = isFollowing ? "followed" : "unfollowed";
            return Ok(new { Message = $"User {status} successfully.", IsFollowing = isFollowing });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("me/following")]
    public async Task<IActionResult> GetFollowing([FromQuery] TechGearAuction.Application.Features.Users.Queries.GetMyFollowingQuery query)
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

    [HttpPost("{id}/block")]
    public async Task<IActionResult> ToggleBlock(Guid id)
    {
        try
        {
            var isBlocked = await _mediator.Send(new TechGearAuction.Application.Features.Users.Commands.ToggleUserBlockCommand { BlockedId = id });
            var status = isBlocked ? "blocked" : "unblocked";
            return Ok(new { Message = $"User {status} successfully.", IsBlocked = isBlocked });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("me/blocked")]
    public async Task<IActionResult> GetBlockedUsers([FromQuery] TechGearAuction.Application.Features.Users.Queries.GetMyBlockedUsersQuery query)
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

