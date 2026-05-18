using MathTestSystem.ApiGateway.Models;
using MathTestSystem.Domain.Constants;
using MathTestSystem.Infrastructure.Auth;
using MathTestSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MathTestSystem.ApiGateway.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
public class AuthController(
    UserManager<AppUser> userManager,
    IJwtTokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        AppUser? user = await userManager.FindByNameAsync(request.Username);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(ResultCodes.InvalidCredentials);

        // Get all roles for this user
        IList<string> userRoles = await userManager.GetRolesAsync(user);
        
        // Use the first role, or default to "User" if no roles assigned (shouldn't happen)
        string role = userRoles.FirstOrDefault() ?? "User";

        string token = tokenService.GenerateToken(user.Id, user.UserName!, role);

        return Ok(new LoginResponse(token, user.UserName!, role));
    }
}
