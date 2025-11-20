using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Harmony.Lambda.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Harmony.Lambda.Tests;

public class PairingEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public PairingEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GeneratePairings_SimpleTwoTeams_Success()
    {
        var request = new PairingRequest
        {
            Teams = new List<TeamRequest>
            {
                new() { Name = "Team A", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 1, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team B", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 2, Club = null, OpponentHistory = new List<string>() }
            },
            RoundNumber = 1
        };

        var response = await _client.PostAsJsonAsync("/api/pairing/generate", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PairingResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Single(result.Matchups);
        Assert.False(result.Matchups[0].IsBye);
        Assert.Contains(result.Matchups[0].Aff, new[] { "Team A", "Team B" });
        Assert.Contains(result.Matchups[0].Neg, new[] { "Team A", "Team B" });
    }

    [Fact]
    public async Task GeneratePairings_ThreeTeams_OneBye()
    {
        var request = new PairingRequest
        {
            Teams = new List<TeamRequest>
            {
                new() { Name = "Team A", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 1, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team B", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 2, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team C", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 3, Club = null, OpponentHistory = new List<string>() }
            },
            RoundNumber = 1
        };

        var response = await _client.PostAsJsonAsync("/api/pairing/generate", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PairingResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Matchups.Count);
        Assert.Single(result.Matchups.Where(m => m.IsBye));
        Assert.Single(result.Matchups.Where(m => !m.IsBye));
    }

    [Fact]
    public async Task GeneratePairings_ByeEligibility_RespectsByeFlag()
    {
        var request = new PairingRequest
        {
            Teams = new List<TeamRequest>
            {
                new() { Name = "Team A", IsByeEligible = false, Wins = 10, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 1, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team B", IsByeEligible = false, Wins = 10, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 2, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team C", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 3, Club = null, OpponentHistory = new List<string>() }
            },
            RoundNumber = 1
        };

        var response = await _client.PostAsJsonAsync("/api/pairing/generate", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PairingResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success);

        // Team C should get the bye since it's the only eligible team
        var byeMatchup = result.Matchups.FirstOrDefault(m => m.IsBye);
        Assert.NotNull(byeMatchup);
        Assert.Equal("Team C", byeMatchup.Aff);
    }

    [Fact]
    public async Task GeneratePairings_PowermatchScenario_PrefersWinBalance()
    {
        var request = new PairingRequest
        {
            Teams = new List<TeamRequest>
            {
                new() { Name = "Team A", IsByeEligible = true, Wins = 2, Losses = 0, AffRounds = 1, NegRounds = 0, Seed = 1, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team B", IsByeEligible = true, Wins = 1, Losses = 1, AffRounds = 1, NegRounds = 0, Seed = 2, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team C", IsByeEligible = true, Wins = 1, Losses = 1, AffRounds = 0, NegRounds = 1, Seed = 3, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team D", IsByeEligible = true, Wins = 0, Losses = 2, AffRounds = 0, NegRounds = 1, Seed = 4, Club = null, OpponentHistory = new List<string>() }
            },
            RoundNumber = 3
        };

        var response = await _client.PostAsJsonAsync("/api/pairing/generate", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PairingResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Matchups.Count);

        // Should pair 2-0 vs 1-1 and 1-1 vs 0-2 (matching wins)
        Assert.Contains(result.Matchups, m =>
            (m.Aff == "Team D" && m.Neg == "Team B") || (m.Aff == "Team B" && m.Neg == "Team D"));
        Assert.Contains(result.Matchups, m =>
            (m.Aff == "Team C" && m.Neg == "Team A") || (m.Aff == "Team A" && m.Neg == "Team C"));
    }

    [Fact]
    public async Task GeneratePairings_OpponentHistory_AvoidsRematches()
    {
        var request = new PairingRequest
        {
            Teams = new List<TeamRequest>
            {
                new() { Name = "Team A", IsByeEligible = true, Wins = 2, Losses = 0, AffRounds = 1, NegRounds = 0, Seed = 1, Club = null, OpponentHistory = new List<string> { "Team C" } },
                new() { Name = "Team B", IsByeEligible = true, Wins = 1, Losses = 1, AffRounds = 1, NegRounds = 0, Seed = 2, Club = null, OpponentHistory = new List<string> { "Team D" } },
                new() { Name = "Team C", IsByeEligible = true, Wins = 1, Losses = 1, AffRounds = 0, NegRounds = 1, Seed = 3, Club = null, OpponentHistory = new List<string> { "Team A" } },
                new() { Name = "Team D", IsByeEligible = true, Wins = 0, Losses = 2, AffRounds = 0, NegRounds = 1, Seed = 4, Club = null, OpponentHistory = new List<string> { "Team B" } }
            },
            RoundNumber = 3
        };

        var response = await _client.PostAsJsonAsync("/api/pairing/generate", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PairingResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Matchups.Count);

        // Should NOT pair A-C or B-D (they already hit)
        Assert.DoesNotContain(result.Matchups, m =>
            (m.Aff == "Team A" && m.Neg == "Team C") || (m.Aff == "Team C" && m.Neg == "Team A"));
        Assert.DoesNotContain(result.Matchups, m =>
            (m.Aff == "Team B" && m.Neg == "Team D") || (m.Aff == "Team D" && m.Neg == "Team B"));
    }

    [Fact]
    public async Task GeneratePairings_AffNegBalance_RespectsConstraints()
    {
        var request = new PairingRequest
        {
            Teams = new List<TeamRequest>
            {
                new() { Name = "Team A", IsByeEligible = true, Wins = 1, Losses = 0, AffRounds = 1, NegRounds = 0, Seed = 1, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team B", IsByeEligible = true, Wins = 0, Losses = 1, AffRounds = 1, NegRounds = 0, Seed = 2, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team C", IsByeEligible = true, Wins = 0, Losses = 1, AffRounds = 0, NegRounds = 1, Seed = 3, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team D", IsByeEligible = true, Wins = 1, Losses = 0, AffRounds = 0, NegRounds = 1, Seed = 4, Club = null, OpponentHistory = new List<string>() }
            },
            RoundNumber = 2
        };

        var response = await _client.PostAsJsonAsync("/api/pairing/generate", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PairingResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(2, result.Matchups.Count);

        // Teams A and B have 1 AFF, 0 NEG - so should go NEG
        // Teams C and D have 0 AFF, 1 NEG - so should go AFF
        Assert.Contains(result.Matchups, m =>
            (m.Aff == "Team D" && m.Neg == "Team A") || (m.Aff == "Team C" && m.Neg == "Team A") ||
            (m.Aff == "Team D" && m.Neg == "Team B") || (m.Aff == "Team C" && m.Neg == "Team B"));
    }

    [Fact]
    public async Task GeneratePairings_HighLowPattern_MaximizesSeedSpread()
    {
        var request = new PairingRequest
        {
            Teams = new List<TeamRequest>
            {
                new() { Name = "Team A", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 1, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team B", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 2, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team C", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 3, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team D", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 4, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team E", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 5, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team F", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 6, Club = null, OpponentHistory = new List<string>() }
            },
            RoundNumber = 1
        };

        var response = await _client.PostAsJsonAsync("/api/pairing/generate", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PairingResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(3, result.Matchups.Count);

        // High-low pattern: 1-6, 2-5, 3-4
        Assert.Contains(result.Matchups, m =>
            (m.Aff == "Team A" && m.Neg == "Team F") || (m.Aff == "Team F" && m.Neg == "Team A"));
        Assert.Contains(result.Matchups, m =>
            (m.Aff == "Team B" && m.Neg == "Team E") || (m.Aff == "Team E" && m.Neg == "Team B"));
        Assert.Contains(result.Matchups, m =>
            (m.Aff == "Team C" && m.Neg == "Team D") || (m.Aff == "Team D" && m.Neg == "Team C"));
    }

    [Fact]
    public async Task GeneratePairings_ImpossiblePairing_ReturnsError()
    {
        var request = new PairingRequest
        {
            Teams = new List<TeamRequest>
            {
                new() { Name = "Team A", IsByeEligible = false, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 1, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team B", IsByeEligible = false, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 2, Club = null, OpponentHistory = new List<string>() },
                new() { Name = "Team C", IsByeEligible = false, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 3, Club = null, OpponentHistory = new List<string>() }
            },
            RoundNumber = 1
        };

        var response = await _client.PostAsJsonAsync("/api/pairing/generate", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PairingResponse>(_jsonOptions);
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Contains("Cannot pair", result.Error);
    }
}
