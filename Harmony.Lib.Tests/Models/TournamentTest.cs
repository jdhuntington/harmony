namespace Harmony.Lib.Tests;
using Harmony.Lib;
using Harmony.Lib.Models;

public class TournamentTest
{
    [Fact]
    public void GenerateRoundSeedsFirstRound()
    {
        var teamA = new Team { Name = "teamA" };
        var teamB = new Team { Name = "teamB" };
        var tournament = new Tournament();
        tournament.AddTeam(teamA);
        tournament.AddTeam(teamB);
        var round = tournament.GenerateRound();
        Assert.Equal(1, round.Number);
        Assert.Single(round.Matchups);
    }

    [Fact]
    public void GenerateRoundSeedsFirstRoundWithTwoTeams()
    {
        var teamA = new Team { Name = "teamA" };
        var teamB = new Team { Name = "teamB" };
        var teamC = new Team { Name = "teamC" };
        var teamD = new Team { Name = "teamD" };
        var tournament = new Tournament();
        tournament.AddTeam(teamA);
        tournament.AddTeam(teamB);
        tournament.AddTeam(teamC);
        tournament.AddTeam(teamD);
        var round = tournament.GenerateRound();
        Assert.Equal(1, round.Number);
        Assert.Equal(2, round.Matchups.Count);
    }
}
