using Harmony.Lib.Models;
using Harmony.Lib.Algorithms;
using Harmony.Simulation;
using SimTeam = Harmony.Simulation.Team;
using TournamentTeam = Harmony.Lib.Models.Team;

// Parse command-line arguments
if (args.Length < 3)
{
    Console.WriteLine("Usage: Harmony.Simulation <numTeams> <numRounds> <numPowerMatchedRounds>");
    Console.WriteLine("Example: Harmony.Simulation 20 6 3");
    return 1;
}

if (!int.TryParse(args[0], out int numTeams) || numTeams < 2)
{
    Console.WriteLine("Error: numTeams must be an integer >= 2");
    return 1;
}

if (!int.TryParse(args[1], out int numRounds) || numRounds < 1)
{
    Console.WriteLine("Error: numRounds must be an integer >= 1");
    return 1;
}

if (!int.TryParse(args[2], out int numPowerMatchedRounds) || numPowerMatchedRounds < 0)
{
    Console.WriteLine("Error: numPowerMatchedRounds must be an integer >= 0");
    return 1;
}

if (numPowerMatchedRounds > numRounds)
{
    Console.WriteLine("Error: numPowerMatchedRounds cannot be greater than numRounds");
    return 1;
}

// Initialize simulation
var random = new Random();
var randomFactory = new RandomFactory();
var judge = new Judge(0.0); // No bias

// Create tournament teams
var tournamentTeams = new List<TournamentTeam>();
var simulationTeams = new Dictionary<string, SimTeam>();

for (int i = 0; i < numTeams; i++)
{
    var teamName = $"Team{i + 1}";
    var tournamentTeam = new TournamentTeam { Name = teamName, Seed = i + 1 };
    tournamentTeams.Add(tournamentTeam);

    // Create corresponding simulation team with random strength and variance
    var strength = random.NextDouble() * 0.6 + 0.2; // Strength between 0.2 and 0.8
    var variance = random.NextDouble() * 0.15 + 0.05; // Variance between 0.05 and 0.2
    simulationTeams[teamName] = new SimTeam(strength, variance);
}

Console.WriteLine($"=== Tournament Simulation ===");
Console.WriteLine($"Teams: {numTeams}");
Console.WriteLine($"Total Rounds: {numRounds}");
Console.WriteLine($"Random Rounds: {numRounds - numPowerMatchedRounds}");
Console.WriteLine($"Power-Matched Rounds: {numPowerMatchedRounds}");
Console.WriteLine();

// Run rounds
var randomMatcher = new RandomMatching(random);
var powerMatcher = new PowermatchHighLow();

for (int round = 1; round <= numRounds; round++)
{
    Console.WriteLine($"=== ROUND {round} ===");

    // Determine which matching strategy to use
    bool usePowerMatching = round > (numRounds - numPowerMatchedRounds);
    var matchingStrategy = usePowerMatching ? "Power-Matched" : "Random";
    Console.WriteLine($"Strategy: {matchingStrategy}");
    Console.WriteLine();

    // Generate pairings
    List<dynamic> edges;
    try
    {
        if (usePowerMatching)
        {
            edges = powerMatcher.SolveMatching(tournamentTeams).Cast<dynamic>().ToList();
        }
        else
        {
            edges = randomMatcher.SolveMatching(tournamentTeams).Cast<dynamic>().ToList();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error generating pairings: {ex.Message}");
        return 1;
    }

    // Sort edges by the win record of the better team (for display purposes)
    var sortedEdges = edges.OrderByDescending(e =>
    {
        var aff = (TournamentTeam)e.Aff;
        var neg = (TournamentTeam?)e.Neg;
        return neg == null ? aff.Wins : Math.Max(aff.Wins, neg.Wins);
    }).ToList();

    // Display matchups
    Console.WriteLine("Matchups:");
    foreach (var edge in sortedEdges)
    {
        var affTeam = (TournamentTeam)edge.Aff;
        var negTeam = (TournamentTeam?)edge.Neg;

        string GetSideIndicator(TournamentTeam team)
        {
            if (team.NegRounds > team.AffRounds) return "A";
            if (team.NegRounds < team.AffRounds) return "N";
            return "?";
        }

        if (negTeam == null)
        {
            var affIndicator = GetSideIndicator(affTeam);
            Console.WriteLine($"  {affIndicator} {affTeam.Name,-10} ({affTeam.Wins}-{affTeam.Losses}) - BYE");
        }
        else
        {
            var affIndicator = GetSideIndicator(affTeam);
            var negIndicator = GetSideIndicator(negTeam);
            var separator = affTeam.Wins == negTeam.Wins ? "-" : "*";
            Console.WriteLine($"  {affIndicator} {affTeam.Name,-10} ({affTeam.Wins}-{affTeam.Losses}) {separator} {negIndicator} {negTeam.Name,-10} ({negTeam.Wins}-{negTeam.Losses})");
        }
    }
    Console.WriteLine();

    // Execute matchups and display results
    Console.WriteLine("Results:");
    foreach (var edge in edges)
    {
        var affTeam = (TournamentTeam)edge.Aff;
        var negTeam = (TournamentTeam?)edge.Neg;

        if (negTeam == null)
        {
            // Bye round
            affTeam.RecordBye(round);
            affTeam.Wins++;
            Console.WriteLine($"  {affTeam.Name,-10} (BYE) - now {affTeam.Wins}-{affTeam.Losses}");
        }
        else
        {
            // Record side assignments
            affTeam.RecordAff(round);
            negTeam.RecordNeg(round);

            // Record opponents
            affTeam.RecordOpponent(negTeam);
            negTeam.RecordOpponent(affTeam);

            // Simulate the matchup
            var affSimTeam = simulationTeams[affTeam.Name];
            var negSimTeam = simulationTeams[negTeam.Name];
            var matchup = new Harmony.Simulation.Matchup(judge, affSimTeam, negSimTeam);
            var result = matchup.Evaluate(randomFactory);

            // Record results
            if (result == MatchupResult.Aff)
            {
                affTeam.Wins++;
                negTeam.Losses++;
                Console.WriteLine($"  {affTeam.Name,-10} (Aff) defeats {negTeam.Name,-10} (Neg) - now {affTeam.Wins}-{affTeam.Losses} and {negTeam.Wins}-{negTeam.Losses}");
            }
            else
            {
                negTeam.Wins++;
                affTeam.Losses++;
                Console.WriteLine($"  {negTeam.Name,-10} (Neg) defeats {affTeam.Name,-10} (Aff) - now {negTeam.Wins}-{negTeam.Losses} and {affTeam.Wins}-{affTeam.Losses}");
            }
        }
    }

    Console.WriteLine();
}

// Final standings
Console.WriteLine("=== FINAL STANDINGS ===");
var sortedTeams = tournamentTeams.OrderByDescending(t => t.Wins).ThenBy(t => t.Losses).ToList();
int rank = 1;
foreach (var team in sortedTeams)
{
    var simTeam = simulationTeams[team.Name];
    Console.WriteLine($"{rank}. {team.Name}: {team.Wins}-{team.Losses} (Aff: {team.AffRounds}, Neg: {team.NegRounds}) [Strength: {simTeam.Strength:F3}]");
    rank++;
}

return 0;
