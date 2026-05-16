using MathTestSystem.GradingService.Endpoints;
using MathTestSystem.GradingService.Parsing;
using MathTestSystem.GradingService.Services;
using MathTestSystem.Infrastructure.Data;
using MathTestSystem.Infrastructure.Extensions;
using MathTestSystem.MathProcessor.Interfaces;
using MathTestSystem.MathProcessor.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
builder.Services.AddScoped<IExamXmlParser, ExamXmlParser>();
builder.Services.AddScoped<IGradingService, ExamGradingService>();

WebApplication app = builder.Build();

// Apply pending migrations on startup so the database is always up to date
using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapExamEndpoints();

app.Run();
