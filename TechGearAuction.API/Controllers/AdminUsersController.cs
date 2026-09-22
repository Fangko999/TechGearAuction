using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.Users.Commands;
using TechGearAuction.Application.Features.Users.Queries;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query)
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
    public async Task<IActionResult> GetUserById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetUserByIdQuery { Id = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UpdateUserByAdminCommand command)
    {
        if (id != command.TargetUserId)
        {
            return BadRequest(new { Message = "ID mismatch." });
        }

        try
        {
            await _mediator.Send(command);
            return Ok(new { Message = "User profile updated successfully by Admin." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> CloseAccount(Guid id)
    {
        try
        {
            await _mediator.Send(new CloseUserAccountCommand { TargetUserId = id });
            return Ok(new { Message = "User account closed successfully." });
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

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreAccount(Guid id)
    {
        try
        {
            await _mediator.Send(new RestoreUserAccountCommand { TargetUserId = id });
            return Ok(new { Message = "User account restored successfully." });
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

    [HttpPut("{id}/ban")]
    public async Task<IActionResult> BanUser(Guid id, [FromBody] BanUserCommand command)
    {
        if (id != command.TargetUserId)
        {
            return BadRequest(new { Message = "ID mismatch." });
        }

        try
        {
            await _mediator.Send(command);
            return Ok(new { Message = "User banned successfully." });
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

    [HttpPut("{id}/unban")]
    public async Task<IActionResult> UnbanUser(Guid id, [FromBody] UnbanUserCommand command)
    {
        if (id != command.TargetUserId)
        {
            return BadRequest(new { Message = "ID mismatch." });
        }

        try
        {
            await _mediator.Send(command);
            return Ok(new { Message = "User unbanned successfully." });
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
}



