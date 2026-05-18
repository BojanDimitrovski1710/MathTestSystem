using MathTestSystem.Infrastructure.Data;
using MathTestSystem.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024); // 100 MB

builder.Services.AddControllers();
builder.Services.AddOpenApi();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

WebApplication app = builder.Build();

// Run migrations on startup
using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();

app.Run();
