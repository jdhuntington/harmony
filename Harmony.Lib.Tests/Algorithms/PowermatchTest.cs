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
        Assert.Single(round.Matchups, m => m.Aff == teamD && m.Neg == teamA);
        Assert.Single(round.Matchups, m => m.Aff == teamC && m.Neg == teamB);
    }

    [Fact]
    public void PowermatchPrefersToSmallPullupsOverABigPullupTest()
    {
        var teamA = new Team { Name = "teamA", Wins = 2, Losses = 0, AffRounds = 1, NegRounds = 0 };
        var teamB = new Team { Name = "teamB", Wins = 1, Losses = 1, AffRounds = 1, NegRounds = 0 };
        var teamC = new Team { Name = "teamC", Wins = 1, Losses = 1, AffRounds = 0, NegRounds = 1 };
        var teamD = new Team { Name = "teamD", Wins = 0, Losses = 2, AffRounds = 0, NegRounds = 1 };
        var round = new Round { Number = 3 };
        round.PowermatchHighLow([teamA, teamB, teamC, teamD]);
        Assert.Equal(2, round.Matchups.Count);
        Assert.Single(round.Matchups, m => m.Aff == teamD && m.Neg == teamB);
        Assert.Single(round.Matchups, m => m.Aff == teamC && m.Neg == teamA);
    }

    [Fact]
    public void PowermatchGivesABye()
    {
        var teamA = new Team { Name = "teamA", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0 };
        var teamB = new Team { Name = "teamB", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0 };
        var teamC = new Team { Name = "teamC", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0 };
        var round = new Round { Number = 1 };
        round.PowermatchHighLow([teamA, teamB, teamC]);
        Assert.Equal(2, round.Matchups.Count);
        Assert.Single(round.Matchups, m => m.IsBye);
    }

    [Fact]
    public void WorstTeamGetsBye()
    {
        var teamA = new Team { Name = "teamA", Wins = 10, Losses = 0, AffRounds = 0, NegRounds = 0 };
        var teamB = new Team { Name = "teamB", Wins = 10, Losses = 0, AffRounds = 0, NegRounds = 0 };
        var teamC = new Team { Name = "teamC", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0 };
        var round = new Round { Number = 1 };
        round.PowermatchHighLow([teamA, teamB, teamC]);
        Assert.Equal(2, round.Matchups.Count);
        Assert.Single(round.Matchups, m => m.IsBye && m.Aff == teamC);
    }

    [Fact]
    public void WorstTeamWithoutExistingByeGetsBye()
    {
        var teamA = new Team { Name = "teamA", Wins = 10, Losses = 0, AffRounds = 0, NegRounds = 0 };
        var teamB = new Team { Name = "teamB", Wins = 9, Losses = 0, AffRounds = 0, NegRounds = 0 };
        var teamC = new Team { Name = "teamC", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0 };
        teamC.RecordBye(1);
        var round = new Round { Number = 1 };
        round.PowermatchHighLow([teamA, teamB, teamC]);
        Assert.Equal(2, round.Matchups.Count);
        Assert.Single(round.Matchups, m => m.IsBye && m.Aff == teamB);
    }

    [Fact]
    public void TeamsCannotHitPrior()
    {
        var teamA = new Team { Name = "teamA", Wins = 2, Losses = 0, AffRounds = 1, NegRounds = 0 };
        var teamB = new Team { Name = "teamB", Wins = 1, Losses = 1, AffRounds = 1, NegRounds = 0 };
        var teamC = new Team { Name = "teamC", Wins = 1, Losses = 1, AffRounds = 0, NegRounds = 1 };
        var teamD = new Team { Name = "teamD", Wins = 0, Losses = 2, AffRounds = 0, NegRounds = 1 };
        teamA.RecordOpponent(teamC);
        teamC.RecordOpponent(teamA);
        teamB.RecordOpponent(teamD);
        teamD.RecordOpponent(teamB);
        var round = new Round { Number = 3 };
        round.PowermatchHighLow([teamA, teamB, teamC, teamD]);
        Assert.Equal(2, round.Matchups.Count);
        Assert.Single(round.Matchups, m => m.Aff == teamC && m.Neg == teamB);
        Assert.Single(round.Matchups, m => m.Aff == teamD && m.Neg == teamA);
    }

    [Fact]
    public void HighLowTest()
    {
        var teamA = new Team { Name = "teamA", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 1 };
        var teamB = new Team { Name = "teamB", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 2 };
        var teamC = new Team { Name = "teamC", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 3 };
        var teamD = new Team { Name = "teamD", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 4 };
        var teamE = new Team { Name = "teamE", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 5 };
        var teamF = new Team { Name = "teamF", Wins = 0, Losses = 0, AffRounds = 0, NegRounds = 0, Seed = 6 };
        var round = new Round { Number = 1 };
        round.PowermatchHighLow([teamA, teamB, teamC, teamD, teamE, teamF]);
        Assert.Equal(3, round.Matchups.Count);
        Assert.Single(round.Matchups, m => m.Contains(teamA) && m.Contains(teamF));
        Assert.Single(round.Matchups, m => m.Contains(teamB) && m.Contains(teamE));
        Assert.Single(round.Matchups, m => m.Contains(teamD) && m.Contains(teamD));
    }

    [Fact]
    public void ShouldNotPairIfByeIsNeededButNoOneEligible()
    {
        var teamA = new Team { Name = "teamA" };
        var teamB = new Team { Name = "teamB" };
        var teamC = new Team { Name = "teamC" };
        teamA.RecordBye(1);
        teamB.RecordBye(1);
        teamC.RecordBye(1);
        Assert.Throws<CannotPairException>(() => (new Round { Number = 1 }).PowermatchHighLow([teamA, teamB, teamC]));
    }
}