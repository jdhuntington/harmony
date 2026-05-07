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

    public double OpponentStrength =>
        Opponents.Count == 0 ? 0.0 : (double)Opponents.Sum(o => o.Wins) / Opponents.Count;

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
        // 1. Minimize win difference (primary goal) - 2+ win gaps should be essentially forbidden
        // 2. Maximize seed spread (high-low pairing) - square the spread to heavily favor larger spreads
        // 3. Avoid same-club matchups
        var winDiff = Math.Abs(Wins - negTeam.Wins);
        var seedSpread = Math.Abs(Seed - negTeam.Seed);

        // Use tiered costs to avoid overflow while making 2+ win gaps prohibitive
        // 0-win: 0, 1-win: 100,000, 2-win: 10,000,000, 3-win: 100,000,000, 4+win: 500,000,000
        int winCost = winDiff switch
        {
            0 => 0,
            1 => 100_000,
            2 => 10_000_000,
            3 => 100_000_000,
            _ => 500_000_000
        };

        // Prefer larger seed spreads - square the spread and negate it
        // spread=1: cost=10000-1=9999, spread=3: cost=10000-9=9991, spread=5: cost=10000-25=9975
        // This heavily incentivizes maximizing individual matchup spreads (high-low fold pattern)
        var seedCost = 10000 - (seedSpread * seedSpread);

        var clubPenalty = (this.Club == negTeam.Club && this.Club != null) ? 100 : 0;

        // Pull-up: prefer lower team with weakest schedule. Scale sits above seedCost (≤10k) and below winDiff=1 jump (100k), so it never overrides bracket-distance.
        int pullUpCost = 0;
        if (winDiff > 0)
        {
            var lower = Wins < negTeam.Wins ? this : negTeam;
            pullUpCost = (int)Math.Round(lower.OpponentStrength * 1000);
        }

        return winCost + seedCost + clubPenalty + pullUpCost;
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