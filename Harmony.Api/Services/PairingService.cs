using Harmony.Api.Models;
using Harmony.Lib.Models;

namespace Harmony.Api.Services;

public class PairingService
{
    public PairingResponse GeneratePairings(PairingRequest request)
    {
        try
        {
            // Create a mapping of team names to Team objects for opponent history lookup
            var teamsByName = new Dictionary<string, Team>();
            var teams = new List<Team>();

            // First pass: Create all team objects
            foreach (var teamRequest in request.Teams)
            {
                var team = new Team
                {
                    Name = teamRequest.Name,
                    Wins = teamRequest.Wins,
                    Losses = teamRequest.Losses,
                    AffRounds = teamRequest.AffRounds,
                    NegRounds = teamRequest.NegRounds,
                    Seed = teamRequest.Seed,
                    Club = teamRequest.Club
                };

                teams.Add(team);
                teamsByName[team.Name] = team;
            }

            // Second pass: Populate opponent history
            for (int i = 0; i < request.Teams.Count; i++)
            {
                var teamRequest = request.Teams[i];
                var team = teams[i];

                foreach (var opponentName in teamRequest.OpponentHistory)
                {
                    if (teamsByName.TryGetValue(opponentName, out var opponent))
                    {
                        team.RecordOpponent(opponent);
                    }
                }
            }

            // Filter to only bye-eligible teams for the algorithm
            var byeEligibleTeamNames = request.Teams
                .Where(t => t.IsByeEligible)
                .Select(t => t.Name)
                .ToHashSet();

            // Remove bye edges for non-eligible teams by marking them as having had a bye
            // This is a workaround since PowermatchHighLow checks HadBye property
            foreach (var team in teams)
            {
                if (!byeEligibleTeamNames.Contains(team.Name))
                {
                    // Mark as having had a bye so they won't be considered for byes
                    team.RecordBye(-1);
                }
            }

            // Create round and generate pairings
            var round = new Round { Number = request.RoundNumber };
            round.PowermatchHighLow(teams);

            // Convert matchups to response format
            var matchupResponses = round.Matchups.Select(m => new MatchupResponse
            {
                Aff = m.Aff.Name,
                Neg = m.Neg?.Name,
                IsBye = m.IsBye
            }).ToList();

            return new PairingResponse
            {
                Matchups = matchupResponses,
                Success = true,
                Error = null
            };
        }
        catch (Exception ex)
        {
            return new PairingResponse
            {
                Matchups = new List<MatchupResponse>(),
                Success = false,
                Error = ex.Message
            };
        }
    }
}
