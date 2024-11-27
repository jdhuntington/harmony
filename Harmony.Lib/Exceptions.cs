using Harmony.Lib.Models;

namespace Harmony.Lib;

public class TooManyByesException(Team team)
    : Exception($"{team.Name} already had a bye in round {team.ByeRound}");

public class ImbalancedRoundsException(Team team)
    : Exception($"{team.Name} has had {team.AffRounds} aff rounds and {team.NegRounds} neg rounds");

public class CannotPairException() : Exception("Cannot pair teams");