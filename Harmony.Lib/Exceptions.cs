using Harmony.Lib.Models;

namespace Harmony.Lib;

public class TooManyByesException(Team team)
    : Exception($"{team.Name} already had a bye in round {team.ByeRound}");

public class ImbalancedRoundsException(string teamName, int affRounds, int negRounds)
    : Exception($"{teamName} has had {affRounds} aff rounds and {negRounds} neg rounds");