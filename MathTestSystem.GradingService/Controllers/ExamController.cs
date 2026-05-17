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
public class ExamController(IGradingService gradingService) : ControllerBase
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
            return BadRequest(ResultCodes.RequestBodyEmpty);

        try
        {
            GradeExamResponse response = await gradingService.GradeAsync(xml);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
