namespace MathTestSystem.ApiGateway.Extensions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;
using MathTestSystem.Infrastructure.Data;

public static class IdentitySeedingExtension
{
    public static async Task SeedDefaultIdentityAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        string[] roles = ["Admin", "Teacher", "Student"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "admin@system.local";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var adminUser = new AppUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true
            };

            // Create the user with a hashed password
            var result = await userManager.CreateAsync(adminUser, "admin");

            // If successful, assign the first role ("Admin")
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, roles[0]);
            }
        }
    }
}