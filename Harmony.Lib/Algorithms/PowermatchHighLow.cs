using Google.OrTools.Sat;
using Harmony.Lib.Models;
public class PowermatchHighLow
{
    public class Edge
    {
        public int Team1 { get; set; }
        public int Team2 { get; set; }
        public BoolVar IsSelected { get; set; }

        public override string ToString() => $"Team {Team1} vs Team {Team2}";
    }

    public List<Edge> SolveMatching(int teamCount)
    {
        // Verify we have an even number of teams
        if (teamCount % 2 != 0)
        {
            throw new ArgumentException("Team count must be even");
        }

        var model = new CpModel();

        // Create edges for every possible pairing
        var edges = new List<Edge>();
        for (int i = 0; i < teamCount; i++)
        {
            for (int j = i + 1; j < teamCount; j++)
            {
                edges.Add(new Edge
                {
                    Team1 = i,
                    Team2 = j,
                    IsSelected = model.NewBoolVar($"match_{i}_{j}")
                });
            }
        }

        // Each team must appear in exactly one selected edge
        for (int team = 0; team < teamCount; team++)
        {
            var teamEdges = new List<ILiteral>();
            foreach (var edge in edges)
            {
                if (edge.Team1 == team || edge.Team2 == team)
                {
                    teamEdges.Add(edge.IsSelected);
                }
            }
            model.Add(LinearExpr.Sum(teamEdges) == 1);
        }

        // Total number of selected edges should be teamCount/2
        var allEdges = edges.Select(e => e.IsSelected).ToList();
        model.Add(LinearExpr.Sum(allEdges) == teamCount / 2);

        // Solve the model
        var solver = new CpSolver();
        var status = solver.Solve(model);

        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            // Return only the selected edges
            return edges.Where(e => solver.BooleanValue(e.IsSelected)).ToList();
        }

        throw new InvalidOperationException("No solution found");
    }
}
