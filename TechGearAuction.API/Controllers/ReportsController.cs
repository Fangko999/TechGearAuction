using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReports([FromQuery] TechGearAuction.Application.Features.Reports.Queries.GetMyReportsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] TechGearAuction.Application.Features.Reports.Commands.CreateReportCommand command)
    {
        var resultId = await _mediator.Send(command);
        return Ok(new { Message = "Report created successfully.", ReportId = resultId });
    }
}

