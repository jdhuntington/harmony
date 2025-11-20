namespace Harmony.Lambda.Models;

public class PairingRequest
{
    public required List<TeamRequest> Teams { get; init; }
    public required int RoundNumber { get; init; }
}
