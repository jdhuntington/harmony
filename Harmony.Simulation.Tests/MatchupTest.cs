namespace Harmony.Simulation.Tests;

class UnrandomFactory : IRandomFactory
{
    public IRandomDouble BuildRandom(double mean, double stdDev)
    {
        return new PredictableRandom(mean - stdDev);
        
    }
}

public class MatchupTest
{
    [Fact]
    public void StrongAffTeamBeatsWeakTeam()
    {
        var judge = new Judge(0.5);
        var aff = new Team(1, 0);
        var neg = new Team(0, 0);
        var matchup = new Matchup(judge, aff, neg);
        Assert.Equal(MatchupResult.Aff, matchup.Evaluate(new UnrandomFactory()));
    }

    [Fact]
    public void StrongNegTeamBeatsWeakTeam()
    {
        var judge = new Judge(0.5);
        var aff = new Team(0, 0);
        var neg = new Team(1, 0);
        var matchup = new Matchup(judge, aff, neg);
        Assert.Equal(MatchupResult.Neg, matchup.Evaluate(new UnrandomFactory()));
    }

    [Fact]
    public void BetterTeamCanLoseWithVariance()
    {
        var judge = new Judge(0.5);
        var aff = new Team(1, 1);
        var neg = new Team(0.5, 0);
        var matchup = new Matchup(judge, aff, neg);
        Assert.Equal(MatchupResult.Neg, matchup.Evaluate(new UnrandomFactory()));
    }
}