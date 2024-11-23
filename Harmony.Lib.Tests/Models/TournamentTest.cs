using Harmony.Lib.Models;

namespace Harmony.Lib.Tests.Models;

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
        Assert.Equal(2, round.Matchups.Count);
    }

    [Fact]
    public void GenerateRoundWithThreeTeamsHasABye()
    {
        var teamA = new Team { Name = "teamA" };
        var teamB = new Team { Name = "teamB" };
        var teamC = new Team { Name = "teamC" };
        var tournament = new Tournament();
        tournament.AddTeam(teamA);
        tournament.AddTeam(teamB);
        tournament.AddTeam(teamC);
        var round = tournament.GenerateRound();
        Assert.Equal(2, round.Matchups.Count);
        Assert.Single(round.Matchups, m => m.IsBye);
    }

    [Fact]
    public void TeamDoesntGetASecondBye()
    {
        var teamA = new Team { Name = "teamA" };
        var teamB = new Team { Name = "teamB" };
        var teamC = new Team { Name = "teamC" };
        var tournament = new Tournament();
        tournament.AddTeam(teamA);
        tournament.AddTeam(teamB);
        tournament.AddTeam(teamC);
        var round1 = new Round();
        round1.AddMatchup(new Matchup { Aff = teamA, Neg = teamB });
        round1.AddMatchup(new Matchup { Aff = teamC });
        tournament.AddRound(round1);

        var round2 = new Round();
        round2.AddMatchup(new Matchup { Aff = teamA, Neg = teamB });
        round2.AddMatchup(new Matchup { Aff = teamC });
        Assert.Throws<TooManyByesException>(() => tournament.AddRound(round2));
    }

    [Fact]
    public void RoundsAreBalanced()
    {
        var teamA = new Team { Name = "teamA" };
        var teamB = new Team { Name = "teamB" };
        var tournament = new Tournament();
        tournament.AddTeam(teamA);
        tournament.AddTeam(teamB);
        var round1 = new Round();
        round1.AddMatchup(new Matchup { Aff = teamA, Neg = teamB });
        tournament.AddRound(round1);

        var round2 = new Round();
        round2.AddMatchup(new Matchup { Aff = teamA, Neg = teamB });
        Assert.Throws<ImbalancedRoundsException>(() => tournament.AddRound(round2));
    }
}