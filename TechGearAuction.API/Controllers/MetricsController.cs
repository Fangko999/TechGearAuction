using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/metrics")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MetricsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("seller/overview")]
    public async Task<IActionResult> GetSellerOverview()
    {
        var result = await _mediator.Send(new TechGearAuction.Application.Features.Users.Queries.GetSellerMetricsOverviewQuery());
        return Ok(result);
    }
}

