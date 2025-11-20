namespace Harmony.Api.Models;

public class PairingResponse
{
    public required List<MatchupResponse> Matchups { get; init; }
    public required bool Success { get; init; }
    public string? Error { get; init; }
}
