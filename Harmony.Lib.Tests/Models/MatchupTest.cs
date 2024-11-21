namespace Harmony.Lib.Tests;
using Harmony.Lib;
using Harmony.Lib.Models;

public class MatchupTest
{
    [Fact]
    public void MatchupIsByeWhenNegIsNull()
    {
        var matchup = new Matchup { Aff = new Team { Name = "foo"}, Neg = null };
        Assert.True(matchup.IsBye);
    }
}