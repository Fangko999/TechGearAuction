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
            var result = await _mediator.Send(query);
            return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
            var result = await _mediator.Send(new GetUserByIdQuery { Id = id });
            return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UpdateUserByAdminCommand command)
    {
        if (id != command.TargetUserId)
        {
            return BadRequest(new { Message = "ID mismatch." });
        }

            await _mediator.Send(command);
            return Ok(new { Message = "User profile updated successfully by Admin." });
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> CloseAccount(Guid id)
    {
            await _mediator.Send(new CloseUserAccountCommand { TargetUserId = id });
            return Ok(new { Message = "User account closed successfully." });
    }

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreAccount(Guid id)
    {
            await _mediator.Send(new RestoreUserAccountCommand { TargetUserId = id });
            return Ok(new { Message = "User account restored successfully." });
    }

    [HttpPut("{id}/ban")]
    public async Task<IActionResult> BanUser(Guid id, [FromBody] BanUserCommand command)
    {
        if (id != command.TargetUserId)
        {
            return BadRequest(new { Message = "ID mismatch." });
        }

            await _mediator.Send(command);
            return Ok(new { Message = "User banned successfully." });
    }

    [HttpPut("{id}/unban")]
    public async Task<IActionResult> UnbanUser(Guid id, [FromBody] UnbanUserCommand command)
    {
        if (id != command.TargetUserId)
        {
            return BadRequest(new { Message = "ID mismatch." });
        }

            await _mediator.Send(command);
            return Ok(new { Message = "User unbanned successfully." });
    }
}



