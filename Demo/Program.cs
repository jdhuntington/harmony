using System;

public static class NormalDistribution
{
    private static readonly Random _rand = new Random();
    private static bool _hasSpare = false;
    private static double _spare;

    /// <summary>
    /// Returns a normally distributed random number with the specified mean and standard deviation.
    /// </summary>
    public static double NextGaussian(double mean = 0, double stdDev = 1)
    {
        if (_hasSpare)
        {
            _hasSpare = false;
            return mean + stdDev * _spare;
        }

        double u, v, s;
        do
        {
            u = _rand.NextDouble() * 2.0 - 1.0;
            v = _rand.NextDouble() * 2.0 - 1.0;
            s = u * u + v * v;
        } while (s >= 1.0 || s == 0.0);

        s = Math.Sqrt(-2.0 * Math.Log(s) / s);
        _spare = v * s;
        _hasSpare = true;
        return mean + stdDev * (u * s);
    }
}


/// <summary>
/// Represents a competitor (or team) in the tournament.
/// </summary>
public class Competitor
{
    /// <summary>
    /// The innate strength of the competitor.
    /// </summary>
    public double InnateStrength { get; }

    /// <summary>
    /// The standard deviation that determines round-to-round performance variance.
    /// </summary>
    public double PerformanceStdDev { get; }

    public Competitor(double innateStrength, double performanceStdDev)
    {
        InnateStrength = innateStrength;
        PerformanceStdDev = performanceStdDev;
    }
}

/// <summary>
/// Represents a judge with a base bias and a round-to-round bias variance.
/// </summary>
public class Judge
{
    /// <summary>
    /// The judge’s base bias (a positive value might favor aff, negative favors neg).
    /// </summary>
    public double BaseBias { get; }

    /// <summary>
    /// The standard deviation of the judge’s bias for each round.
    /// </summary>
    public double BiasStdDev { get; }

    public Judge(double baseBias, double biasStdDev)
    {
        BaseBias = baseBias;
        BiasStdDev = biasStdDev;
    }
}


/// <summary>
/// Possible outcomes of a debate round.
/// </summary>
public enum DebateResult
{
    AffWins,
    NegWins
}

/// <summary>
/// Simulates a single round of debate.
/// </summary>
public class DebateRoundSimulator
{
    /// <summary>
    /// Simulates a round and returns the outcome.
    /// </summary>
    /// <param name="aff">The affirmative competitor.</param>
    /// <param name="neg">The negative competitor.</param>
    /// <param name="judge">The judge for the round.</param>
    /// <returns>DebateResult.AffWins if the computed score is greater than 0, otherwise DebateResult.NegWins.</returns>
    public DebateResult SimulateRound(Competitor aff, Competitor neg, Judge judge)
    {
        // Generate the performance for each competitor.
        double affPerformance = NormalDistribution.NextGaussian(aff.InnateStrength, aff.PerformanceStdDev);
        double negPerformance = NormalDistribution.NextGaussian(neg.InnateStrength, neg.PerformanceStdDev);
        
        // Generate the judge's bias for this round.
        double judgeBias = NormalDistribution.NextGaussian(judge.BaseBias, judge.BiasStdDev);
        
        // Calculate the net advantage for aff.
        double netScore = (affPerformance - negPerformance) + judgeBias;
        
        // If netScore > 0, aff wins; otherwise, neg wins.
        return netScore > 0 ? DebateResult.AffWins : DebateResult.NegWins;
    }
}


class Program
{
    static void Main()
    {
        // Create competitors and a judge.
        // For example, assume aff and neg have innate strengths (say 0.7 and 0.6) 
        // and similar performance variability (say 0.1), and the judge has a slight bias in favor of aff.
        var aff = new Competitor(0.6, 0.1);
        var neg = new Competitor(0.6, 0.1);
        var judge = new Judge(0.00, 0.02); // base bias of +0.05 favoring aff, with small variability

        var simulator = new DebateRoundSimulator();

        // Simulate a number of rounds.
        int affWins = 0, negWins = 0;
        int rounds = 10000;
        for (int i = 0; i < rounds; i++)
        {
            DebateResult result = simulator.SimulateRound(aff, neg, judge);
            if (result == DebateResult.AffWins)
                affWins++;
            else
                negWins++;
        }

        Console.WriteLine($"Out of {rounds} rounds:");
        Console.WriteLine($"Aff wins: {affWins}");
        Console.WriteLine($"Neg wins: {negWins}");
    }
}