using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.Admin.Commands;
using TechGearAuction.Application.Features.Admin.Queries;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/admin/auctions")]
[Authorize(Roles = "Admin")]
public class AdminAuctionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAuctionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuctions([FromQuery] GetAdminAuctionsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/force-cancel")]
    public async Task<IActionResult> ForceCancel(Guid id, [FromBody] string reason)
    {
        try
        {
            await _mediator.Send(new ForceCancelAuctionCommand { AuctionId = id, Reason = reason });
            return Ok(new { Message = "Auction cancelled successfully by Admin." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

