namespace Harmony.Lib.Models;

public class Round
{
    public int Number { get; set; }
    public List<Matchup> Matchups { get; set; } = new();

    public void AddMatchup(Matchup matchup1)
    {
        Matchups.Add(matchup1);
    }

    public void Validate()
    {
        Matchups.ForEach(m => m.Validate());
    }

    public void Record()
    {
        Matchups.ForEach(m => m.Record(this));
    }
}