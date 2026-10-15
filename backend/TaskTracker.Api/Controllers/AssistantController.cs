using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.DTOs;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/assistant")]
public class AssistantController(IAssistantService service) : ControllerBase
{
    [HttpPost("tasks")]
    public async Task<ActionResult<AssistantTaskDraft>> CreateTaskDraft(AssistantRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt)) return BadRequest(new { message = "Describe the task you want to create." });

        try
        {
            return Ok(await service.CreateTaskDraftAsync(request.Prompt, cancellationToken));
        }
        catch (AssistantUnavailableException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
    }
}
