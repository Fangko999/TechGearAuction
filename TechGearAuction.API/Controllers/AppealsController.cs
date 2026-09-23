using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.Appeals.Commands;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/appeals")]
public class AppealsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppealsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("banned-users")]
    public async Task<IActionResult> SubmitBanAppeal([FromForm] string email, [FromForm] string description, [FromForm] List<IFormFile> files)
    {
        try
        {
            var command = new SubmitBanAppealCommand
            {
                Email = email,
                Description = description,
                Evidences = files.Select(f => new AppealEvidenceDto
                {
                    Stream = f.OpenReadStream(),
                    FileName = f.FileName,
                    ContentType = f.ContentType
                }).ToList()
            };

            var id = await _mediator.Send(command);
            return Ok(new { Message = "Appeal submitted successfully.", AppealId = id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
