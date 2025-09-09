namespace Harmony.Simulation;

public class Matchup(Judge judge, Team aff, Team neg)
{
    public MatchupResult Evaluate(IRandomFactory factory)
    {
        var affValue = factory.BuildRandom(aff.Strength, aff.Variance).NextDouble();
        var negValue = factory.BuildRandom(neg.Strength, neg.Variance).NextDouble();
        return affValue > negValue ? MatchupResult.Aff : MatchupResult.Neg;
    }
}