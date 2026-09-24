using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.SuspiciousActivities.Commands;
using TechGearAuction.Application.Features.SuspiciousActivities.Queries;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/admin/suspicious-activities")]
[Authorize(Roles = "Admin")]
public class AdminSuspiciousActivitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminSuspiciousActivitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetActivities([FromQuery] GetSuspiciousActivitiesQuery query)
    {
            var result = await _mediator.Send(query);
            return Ok(result);
    }

    [HttpPut("{id}/review")]
    public async Task<IActionResult> ReviewActivity(Guid id)
    {
            await _mediator.Send(new ReviewSuspiciousActivityCommand { Id = id });
            return Ok(new { Message = "Activity marked as reviewed." });
    }
}

