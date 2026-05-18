using MathTestSystem.Domain.Constants;
using MathTestSystem.GradingService.Models;
using MathTestSystem.GradingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathTestSystem.GradingService.Controllers;

[ApiController]
[Route("api/exams")]
[Authorize]
[Tags("Exams")]
public class ExamController(IGradingService gradingService, ILogger<ExamController> logger) : ControllerBase
{
    [HttpPost("grade")]
    [DisableRequestSizeLimit]
    [Consumes("application/xml", "text/xml", "text/plain")]
    [ProducesResponseType<GradeExamResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GradeExams()
    {
        using StreamReader reader = new(Request.Body);
        string xml = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(xml))
        {
            logger.LogWarning("Grade request rejected — empty request body");
            return BadRequest(ResultCodes.RequestBodyEmpty);
        }

        logger.LogInformation("Grade request received — {Bytes} bytes", xml.Length);

        try
        {
            GradeExamResponse response = await gradingService.GradeAsync(xml);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Grading failed: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }
}
