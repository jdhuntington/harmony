namespace Harmony.Lib.Tests;
using Harmony.Lib.Models;

public class PowermatchTest
{
    [Fact]
    public void SimplePowermatchTest()
    {
        var teamA = new Team { Name = "teamA", Wins = 1, Losses = 0, AffRounds = 1, NegRounds = 0 };
        var teamB = new Team { Name = "teamB", Wins = 0, Losses = 1, AffRounds = 1, NegRounds = 0 };
        var teamC = new Team { Name = "teamC", Wins = 0, Losses = 1, AffRounds = 0, NegRounds = 1 };
        var teamD = new Team { Name = "teamD", Wins = 1, Losses = 0, AffRounds = 0, NegRounds = 1 };
        var round = new Round { Number = 2 };
        round.PowermatchHighLow([teamA, teamB, teamC, teamD]);
        Assert.Equal(2, round.Matchups.Count);
        Assert.Single(round.Matchups, m => m.Neg == teamA && m.Neg == teamD);
        Assert.Single(round.Matchups, m => m.Neg == teamB && m.Neg == teamC);
    }
}