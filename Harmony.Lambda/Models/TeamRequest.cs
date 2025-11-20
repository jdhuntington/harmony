namespace Harmony.Lambda.Models;

public class TeamRequest
{
    public required string Name { get; init; }
    public required bool IsByeEligible { get; init; }
    public required int Wins { get; init; }
    public required int Losses { get; init; }
    public required int AffRounds { get; init; }
    public required int NegRounds { get; init; }
    public required int Seed { get; init; }
    public string? Club { get; init; }
    public required List<string> OpponentHistory { get; init; }
}
