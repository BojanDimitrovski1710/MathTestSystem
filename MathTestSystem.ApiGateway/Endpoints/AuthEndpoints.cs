using MathTestSystem.ApiGateway.Models;
using MathTestSystem.Domain.Constants;
using MathTestSystem.Infrastructure.Auth;
using MathTestSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace MathTestSystem.ApiGateway.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticate and receive a JWT.")
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        UserManager<AppUser> userManager,
        IJwtTokenService tokenService)
    {
        AppUser? user = await userManager.FindByNameAsync(request.Username);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Results.Problem(
                detail: ResultCodes.InvalidCredentials,
                statusCode: StatusCodes.Status401Unauthorized);

        // Role is "Admin" for the seeded admin account, "User" for everyone else.
        // Downstream services can use this for fine-grained authorization if needed.
        string role = request.Username == "admin" ? "Admin" : "User";

        string token = tokenService.GenerateToken(user.Id, user.UserName!, role);

        return Results.Ok(new LoginResponse(token, user.UserName!, role));
    }
}
