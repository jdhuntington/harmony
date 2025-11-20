namespace Harmony.Api.Models;

public class MatchupResponse
{
    public required string Aff { get; init; }
    public string? Neg { get; init; }
    public required bool IsBye { get; init; }
}
