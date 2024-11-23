
namespace Harmony.Lib.Models;

public class Round
{
    public int Number { get; set; }
    public List<Matchup> Matchups { get; set; } = [];

    public void AddMatchup(Matchup matchup)
    {
        Matchups.Add(matchup);
    }

    public void Validate()
    {
        Matchups.ForEach(m => m.Validate());
    }

    public void Record()
    {
        Matchups.ForEach(m => m.Record(this));
    }

    public void PowermatchHighLow(List<Team> teams)
    {
        throw new NotImplementedException();
    }
}