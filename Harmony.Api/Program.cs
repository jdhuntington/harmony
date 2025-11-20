using Harmony.Api.Models;
using Harmony.Api.Services;
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

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<PairingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/pairing/generate", (PairingRequest request, PairingService pairingService, ILogger<Program> logger) =>
{
    logger.LogInformation("Received pairing request for round {RoundNumber} with {TeamCount} teams",
        request.RoundNumber, request.Teams.Count);

    // Log each team's state for debugging
    foreach (var team in request.Teams)
    {
        logger.LogDebug("Team {TeamName}: W-L {Wins}-{Losses}, Aff-Neg {AffRounds}-{NegRounds}, Seed {Seed}, ByeEligible {IsByeEligible}, Opponents {@OpponentHistory}",
            team.Name, team.Wins, team.Losses, team.AffRounds, team.NegRounds, team.Seed, team.IsByeEligible, team.OpponentHistory);
    }

    var response = pairingService.GeneratePairings(request);

    if (response.Success)
    {
        logger.LogInformation("Successfully generated {MatchupCount} matchups for round {RoundNumber}",
            response.Matchups.Count, request.RoundNumber);

        foreach (var matchup in response.Matchups)
        {
            if (matchup.IsBye)
            {
                logger.LogInformation("Matchup: {Aff} receives BYE", matchup.Aff);
            }
            else
            {
                logger.LogInformation("Matchup: {Aff} (AFF) vs {Neg} (NEG)", matchup.Aff, matchup.Neg);
            }
        }

        return Results.Ok(response);
    }
    else
    {
        logger.LogError("Failed to generate pairings for round {RoundNumber}: {Error}",
            request.RoundNumber, response.Error);
        return Results.BadRequest(response);
    }
})
.WithName("GeneratePairings");

app.Run();

// Make the Program class accessible for integration tests
public partial class Program { }
