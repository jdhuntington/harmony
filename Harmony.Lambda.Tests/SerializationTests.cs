using System.Text.Json;
using Harmony.Lambda.Models;

namespace Harmony.Lambda.Tests;

public class SerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void TeamRequest_RoundTrip_PreservesAllData()
    {
        var original = new TeamRequest
        {
            Name = "Team Alpha",
            IsByeEligible = true,
            Wins = 3,
            Losses = 1,
            AffRounds = 2,
            NegRounds = 2,
            Seed = 5,
            Club = "Club X",
            OpponentHistory = new List<string> { "Team B", "Team C", "Team D" }
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<TeamRequest>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.IsByeEligible, deserialized.IsByeEligible);
        Assert.Equal(original.Wins, deserialized.Wins);
        Assert.Equal(original.Losses, deserialized.Losses);
        Assert.Equal(original.AffRounds, deserialized.AffRounds);
        Assert.Equal(original.NegRounds, deserialized.NegRounds);
        Assert.Equal(original.Seed, deserialized.Seed);
        Assert.Equal(original.Club, deserialized.Club);
        Assert.Equal(original.OpponentHistory, deserialized.OpponentHistory);
    }

    [Fact]
    public void TeamRequest_WithNullClub_SerializesCorrectly()
    {
        var original = new TeamRequest
        {
            Name = "Team Solo",
            IsByeEligible = false,
            Wins = 0,
            Losses = 0,
            AffRounds = 0,
            NegRounds = 0,
            Seed = 1,
            Club = null,
            OpponentHistory = new List<string>()
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<TeamRequest>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.Null(deserialized.Club);
        Assert.Empty(deserialized.OpponentHistory);
    }

    [Fact]
    public void TeamRequest_WithTiedSeeds_SerializesCorrectly()
    {
        var teams = new List<TeamRequest>
        {
            new() { Name = "Team A", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 0, Club = null, OpponentHistory = new List<string>() },
            new() { Name = "Team B", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 0, Club = null, OpponentHistory = new List<string>() },
            new() { Name = "Team C", IsByeEligible = true, Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 0, Club = null, OpponentHistory = new List<string>() }
        };

        var json = JsonSerializer.Serialize(teams, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<List<TeamRequest>>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(3, deserialized.Count);
        Assert.All(deserialized, t => Assert.Equal(0, t.Seed));
    }

    [Fact]
    public void PairingRequest_RoundTrip_PreservesAllData()
    {
        var original = new PairingRequest
        {
            Teams = new List<TeamRequest>
            {
                new() { Name = "Team A", IsByeEligible = true, Wins = 2, Losses = 1, AffRounds = 2, NegRounds = 1, Seed = 3, Club = "Club 1", OpponentHistory = new List<string> { "Team B" } },
                new() { Name = "Team B", IsByeEligible = false, Wins = 1, Losses = 2, AffRounds = 1, NegRounds = 2, Seed = 7, Club = null, OpponentHistory = new List<string> { "Team A" } }
            },
            RoundNumber = 4
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<PairingRequest>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(original.RoundNumber, deserialized.RoundNumber);
        Assert.Equal(original.Teams.Count, deserialized.Teams.Count);
        Assert.Equal("Team A", deserialized.Teams[0].Name);
        Assert.Equal("Team B", deserialized.Teams[1].Name);
    }

    [Fact]
    public void MatchupResponse_RoundTrip_PreservesData()
    {
        var original = new MatchupResponse
        {
            Aff = "Team Alpha",
            Neg = "Team Beta",
            IsBye = false
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<MatchupResponse>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Aff, deserialized.Aff);
        Assert.Equal(original.Neg, deserialized.Neg);
        Assert.Equal(original.IsBye, deserialized.IsBye);
    }

    [Fact]
    public void MatchupResponse_Bye_SerializesCorrectly()
    {
        var original = new MatchupResponse
        {
            Aff = "Team Solo",
            Neg = null,
            IsBye = true
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<MatchupResponse>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Aff, deserialized.Aff);
        Assert.Null(deserialized.Neg);
        Assert.True(deserialized.IsBye);
    }

    [Fact]
    public void PairingResponse_Success_RoundTrip()
    {
        var original = new PairingResponse
        {
            Matchups = new List<MatchupResponse>
            {
                new() { Aff = "Team A", Neg = "Team B", IsBye = false },
                new() { Aff = "Team C", Neg = null, IsBye = true }
            },
            Success = true,
            Error = null
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<PairingResponse>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.True(deserialized.Success);
        Assert.Null(deserialized.Error);
        Assert.Equal(2, deserialized.Matchups.Count);
    }

    [Fact]
    public void PairingResponse_Failure_RoundTrip()
    {
        var original = new PairingResponse
        {
            Matchups = new List<MatchupResponse>(),
            Success = false,
            Error = "Cannot pair teams"
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<PairingResponse>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.False(deserialized.Success);
        Assert.Equal("Cannot pair teams", deserialized.Error);
        Assert.Empty(deserialized.Matchups);
    }

    [Fact]
    public void PairingRequest_DeserializesFromCamelCaseJson()
    {
        var json = """
        {
          "teams": [
            {
              "name": "Team A",
              "isByeEligible": true,
              "wins": 2,
              "losses": 1,
              "affRounds": 1,
              "negRounds": 2,
              "seed": 5,
              "club": "Club Alpha",
              "opponentHistory": ["Team B", "Team C"]
            }
          ],
          "roundNumber": 3
        }
        """;

        var deserialized = JsonSerializer.Deserialize<PairingRequest>(json, _jsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(3, deserialized.RoundNumber);
        Assert.Single(deserialized.Teams);
        Assert.Equal("Team A", deserialized.Teams[0].Name);
        Assert.True(deserialized.Teams[0].IsByeEligible);
        Assert.Equal(2, deserialized.Teams[0].Wins);
        Assert.Equal("Club Alpha", deserialized.Teams[0].Club);
        Assert.Equal(2, deserialized.Teams[0].OpponentHistory.Count);
    }
}
