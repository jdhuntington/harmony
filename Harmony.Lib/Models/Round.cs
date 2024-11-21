namespace Harmony.Lib.Models;

public class Round
{
    public int Number { get; set; }
    public List<Matchup> Matchups { get; set; } = new List<Matchup>();

}