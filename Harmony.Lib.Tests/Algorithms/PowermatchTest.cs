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
        Assert.Single(round.Matchups, m => m.Contains(teamC) && m.Contains(teamD));
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
        Assert.Throws<CannotPairException>(() => new Round { Number = 1 }.PowermatchHighLow([teamA, teamB, teamC]));
    }

    public void LargeScaleTournament_110Teams_6Rounds()
    {
        var teams = new List<Team>();

        // Create 110 teams - all starting fresh
        for (int i = 0; i < 110; i++)
        {
            teams.Add(new Team
            {
                Name = $"Team{i:D3}",
                Wins = 0,
                Losses = 0,
                AffRounds = 0,
                NegRounds = 0,
                Seed = i + 1,
                Club = i % 10 == 0 ? $"Club{i / 10}" : null // 10% same club
            });
        }

        var roundTimes = new List<long>();
        var totalSw = System.Diagnostics.Stopwatch.StartNew();

        // Simulate 6 full rounds using the PowermatchHighLow algorithm
        for (int roundNum = 1; roundNum <= 6; roundNum++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var round = new Round { Number = roundNum };
            round.PowermatchHighLow(teams);
            sw.Stop();
            roundTimes.Add(sw.ElapsedMilliseconds);

            // Verify matchups are valid
            Assert.Equal(55, round.Matchups.Count); // 110 teams = 55 matchups
            Assert.All(round.Matchups, m => Assert.False(m.IsBye)); // Even number, no byes
            Assert.All(round.Matchups, m =>
            {
                Assert.False(m.Aff.HasHit(m.Neg));
                Assert.False(m.Neg.HasHit(m.Aff));
            });

            // Record the matchups and randomly assign wins
            var random = new Random(42 + roundNum);
            foreach (var matchup in round.Matchups)
            {
                // Record opponents
                matchup.Aff.RecordOpponent(matchup.Neg);
                matchup.Neg.RecordOpponent(matchup.Aff);

                // Record sides
                matchup.Aff.RecordAff(roundNum);
                matchup.Neg.RecordNeg(roundNum);

                // Randomly assign win
                if (random.Next(2) == 0)
                {
                    matchup.Aff.Wins++;
                    matchup.Neg.Losses++;
                }
                else
                {
                    matchup.Neg.Wins++;
                    matchup.Aff.Losses++;
                }
            }
        }

        totalSw.Stop();

        // Performance check - each round should complete in reasonable time
        for (int i = 0; i < roundTimes.Count; i++)
        {
            Assert.True(roundTimes[i] < 30000,
                $"Round {i + 1} took {roundTimes[i]}ms, expected < 30s");
        }

        Console.WriteLine($"110 teams, 6 rounds completed:");
        for (int i = 0; i < roundTimes.Count; i++)
        {
            Console.WriteLine($"  Round {i + 1}: {roundTimes[i]}ms");
        }
        Console.WriteLine($"Total time: {totalSw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average per round: {roundTimes.Average():F0}ms");
    }
}