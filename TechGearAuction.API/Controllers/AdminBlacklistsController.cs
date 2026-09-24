using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.Admin.Commands;
using TechGearAuction.Application.Features.Admin.Queries;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/admin/blacklists")]
[Authorize(Roles = "Admin")]
public class AdminBlacklistsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminBlacklistsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetBlacklists([FromQuery] GetBlacklistsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpDelete("{hash}")]
    public async Task<IActionResult> RemoveFromBlacklist(string hash)
    {
            await _mediator.Send(new RemoveFromBlacklistCommand { DeviceHash = hash });
            return Ok(new { Message = "Device removed from blacklist successfully." });
    }
}

