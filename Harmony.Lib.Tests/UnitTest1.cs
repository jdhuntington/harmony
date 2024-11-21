namespace Harmony.Lib.Tests;
using Harmony.Lib;
using Harmony.Lib.Models;

public class UnitTest1
{
    [Fact]
    public void AddWorks()
    {
        Assert.Equal(4, new Class1().Add(2, 2));
    }

    [Fact]
    public void AddWorksAlso()
    {
        Assert.Equal(14, new Class1().Add(12, 2));
    }

    [Fact]
    public void MatchupIsByeWhenNegIsNull()
    {
        var matchup = new Matchup { Aff = new Team { Name = "foo"}, Neg = null };
        Assert.True(matchup.IsBye);
    }
}
