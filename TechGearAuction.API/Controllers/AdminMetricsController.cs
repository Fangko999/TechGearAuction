using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.Admin.Queries;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/admin/metrics")]
[Authorize(Roles = "Admin")]
public class AdminMetricsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminMetricsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var result = await _mediator.Send(new GetMetricsOverviewQuery());
        return Ok(result);
    }

    [HttpGet("revenue-chart")]
    public async Task<IActionResult> GetRevenueChart([FromQuery] int days = 30)
    {
        var result = await _mediator.Send(new GetRevenueChartQuery { Days = days });
        return Ok(result);
    }
}

