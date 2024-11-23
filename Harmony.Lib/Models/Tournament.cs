namespace Harmony.Lib.Models;

public class Tournament
{
    private List<Team> Teams { get; } = [];
    private List<Round> Rounds { get; } = [];

    public void AddTeam(Team team)
    {
        Teams.Add(team);
    }

    public Round GenerateRound()
    {
        var matchups = new List<Matchup>();
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

    public void AddRound(Round round1)
    {
        round1.Validate();
        Rounds.Add(round1);
        round1.Record();
    }
}