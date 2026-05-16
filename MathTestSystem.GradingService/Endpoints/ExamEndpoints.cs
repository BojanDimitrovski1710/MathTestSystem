using MathTestSystem.Domain.Constants;
using MathTestSystem.GradingService.Models;
using MathTestSystem.GradingService.Services;

namespace MathTestSystem.GradingService.Endpoints;

public static class ExamEndpoints
{
    public static void MapExamEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/exams")
            .WithTags("Exams");

        group.MapPost("/grade", GradeExams)
            .WithName("GradeExams")
            .WithSummary("Grade a teacher-uploaded XML exam submission.")
            .Accepts<string>("application/xml", "text/xml", "text/plain")
            .Produces<GradeExamResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static async Task<IResult> GradeExams(
        HttpRequest request,
        IGradingService gradingService)
    {
        using StreamReader reader = new(request.Body);
        string xml = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(xml))
            return Results.BadRequest(ResultCodes.RequestBodyEmpty);

        try
        {
            GradeExamResponse response = await gradingService.GradeAsync(xml);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }
}
