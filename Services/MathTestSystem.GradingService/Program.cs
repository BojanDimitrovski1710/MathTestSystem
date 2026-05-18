using MathTestSystem.GradingService.Parsing;
using MathTestSystem.GradingService.Services;
using MathTestSystem.Infrastructure.Extensions;
using MathTestSystem.MathProcessor.Interfaces;
using MathTestSystem.MathProcessor.Services;
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
builder.Services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
builder.Services.AddScoped<IExamXmlParser, ExamXmlParser>();
builder.Services.AddScoped<IGradingService, ExamGradingService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
