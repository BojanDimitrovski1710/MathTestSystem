using MathTestSystem.GradingService.Endpoints;
using MathTestSystem.GradingService.Parsing;
using MathTestSystem.GradingService.Services;
using MathTestSystem.Infrastructure.Extensions;
using MathTestSystem.MathProcessor.Interfaces;
using MathTestSystem.MathProcessor.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
builder.Services.AddScoped<IExamXmlParser, ExamXmlParser>();
builder.Services.AddScoped<IGradingService, ExamGradingService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapExamEndpoints();

app.Run();
