namespace Harmony.Lib.Models;

public class Matchup
{
    public required Team Aff { get; set; }
    public Team? Neg { get; set; }

    public bool IsBye => Neg == null;
}
