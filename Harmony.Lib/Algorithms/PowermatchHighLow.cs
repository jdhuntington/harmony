using Google.OrTools.Sat;
using Harmony.Lib;
using Harmony.Lib.Models;
public class PowermatchHighLow
{
    public class Edge
    {
        public Team Aff { get; init; }
        public Team? Neg { get; init; }
        public BoolVar IsSelected { get; set; }
        public int Cost { get; set; }

        public override string ToString() => $"Team {Aff.Name} vs Team {Neg?.Name ?? "Bye"}";
    }

    public List<Edge> SolveMatching(List<Team> teams)
    {
        var teamCount = teams.Count;
        var byeRoundExists = teamCount % 2 != 0;

        var model = new CpModel();

        var edges = new List<Edge>();
        teams.ForEach(affTeam =>
        {
            if (byeRoundExists && !affTeam.HadBye)
            {
                var byeEdge = new Edge
                {
                    Aff = affTeam,
                    Neg = null,
                    IsSelected = model.NewBoolVar($"bye_{affTeam.Name}"),
                    Cost = affTeam.Wins << 20
                };
                edges.Add(byeEdge);
            }
            if (affTeam.CanGoAff)
            {
                teams.ForEach(negTeam =>
                {
                    if (affTeam != negTeam && negTeam.CanGoNeg && !affTeam.HasHit(negTeam))
                    {
                        var edge = new Edge
                        {
                            Aff = affTeam,
                            Neg = negTeam,
                            IsSelected = model.NewBoolVar($"match_{affTeam.Name}_{negTeam.Name}"),
                            Cost = affTeam.MatchupCost(negTeam)
                        };
                        edges.Add(edge);
                    }
                });
            }
        });

        teams.ForEach(team =>
        {
            var teamEdges = edges.Where(e => e.Aff == team || e.Neg == team).Select(e => e.IsSelected).ToList();
            model.Add(LinearExpr.Sum(teamEdges) == 1);
        });

        var allEdges = edges.Select(e => e.IsSelected).ToList();
        var expectedMatchupCount = teamCount / 2;
        if (byeRoundExists)
        {
            expectedMatchupCount++;
        }
        model.Add(LinearExpr.Sum(allEdges) == expectedMatchupCount);

        var costTerms = edges.Select(e => e.IsSelected * e.Cost).ToList();
        model.Minimize(LinearExpr.Sum(costTerms));

        // Solve the model
        var solver = new CpSolver();
        var status = solver.Solve(model);

        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            // Return only the selected edges
            return edges.Where(e => solver.BooleanValue(e.IsSelected)).ToList();
        }

        throw new CannotPairException();
    }
}
