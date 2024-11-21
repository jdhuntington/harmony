namespace Harmony.Lib.Models;

public class Tournament
{
    public List<Team> Teams { get; } = new List<Team>();

    public void AddTeam(Team team)
    {
        Teams.Add(team);
    }

    public Round GenerateRound()
    {
        var matchups = new List<Matchup>();
        // split teams into twos
        for (var i = 0; i < Teams.Count; i += 2)
        {
            matchups.Add(new Matchup
            {
                Aff = Teams[i],
                Neg = i + 1 < Teams.Count ? Teams[i + 1] : null
            });
        }
        return new Round
        {
            Number = 1,
            Matchups = matchups
        };
    }
}