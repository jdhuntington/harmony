namespace Harmony.Lib.Models;

public class Team
{
    public required string Name { get; init; }
    public int? ByeRound { get; private set; }
    public bool HadBye => ByeRound != null;

    public int AffRounds { get; set; }
    public int NegRounds { get; set; }

    public int Losses { get; set; }
    public int Wins { get; set; }
    public bool CanGoAff => AffRounds <= NegRounds;
    public bool CanGoNeg => NegRounds <= AffRounds;

    public List<Team> Opponents { get; set; } = [];
    public int Seed { get; set; }
    
    public string? Club { get; set; }

    public override string ToString()
    {
        return $"{Name} ({Wins}-{Losses})";
    }

    public void RecordBye(int round)
    {
        ByeRound = round;
    }

    public void RecordAff(int roundNumber)
    {
        AffRounds++;
        CheckRoundBalance();
    }

    public void RecordNeg(int roundNumber)
    {
        NegRounds++;
        CheckRoundBalance();
    }

    private void CheckRoundBalance()
    {
        if (Math.Abs(AffRounds - NegRounds) > 1)
        {
            throw new ImbalancedRoundsException(this);
        }
    }

    internal int MatchupCost(Team negTeam)
    {
        // Lower cost is better. We want:
        // 1. Minimize win difference (primary goal) - use squared penalty so double pull-ups are VERY expensive
        // 2. Maximize seed spread (high-low pairing) - square the spread to heavily favor larger spreads
        // 3. Avoid same-club matchups
        var winDiff = Math.Abs(Wins - negTeam.Wins);
        var seedSpread = Math.Abs(Seed - negTeam.Seed);

        // Square the win difference: 1-win gap = 1000, 2-win gap = 4000, 3-win gap = 9000
        var winCost = 1000 * winDiff * winDiff;

        // Prefer larger seed spreads - square the spread and negate it
        // spread=1: cost=10000-1=9999, spread=3: cost=10000-9=9991, spread=5: cost=10000-25=9975
        // This heavily incentivizes maximizing individual matchup spreads (high-low fold pattern)
        var seedCost = 10000 - (seedSpread * seedSpread);

        var clubPenalty = (this.Club == negTeam.Club && this.Club != null) ? 100 : 0;

        return winCost + seedCost + clubPenalty;
    }

    public void RecordOpponent(Team opponent)
    {
        Opponents.Add(opponent);
    }

    public bool HasHit(Team opponent)
    {
        return Opponents.Contains(opponent);
    }
}