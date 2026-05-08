using Harmony.Lambda.Models;
using Harmony.Lambda.Services;
using Serilog;
using Serilog.Formatting.Compact;

// Configure Serilog for JSON structured logging to stdout
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddSingleton<PairingService>();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();

// Handle CORS preflight requests explicitly for Lambda Function URLs
app.MapMethods("/api/pairing/generate", new[] { "OPTIONS" }, () => Results.Ok())
    .WithName("PreflightGenerate");

app.MapPost("/api/pairing/generate", (PairingRequest request, PairingService pairingService, ILogger<Program> logger) =>
{
    var response = pairingService.GeneratePairings(request);

    logger.LogInformation("Pairing request handled {@Request} {@Response}", request, response);

    return response.Success ? Results.Ok(response) : Results.BadRequest(response);
})
.WithName("GeneratePairings");

app.Run();

// Make the Program class accessible for integration tests
public partial class Program { }
