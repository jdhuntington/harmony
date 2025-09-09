namespace Harmony.Simulation;

public class Team(double strength, double variance)
{
    public double Variance { get; set; } = variance;

    public double Strength { get; set; } = strength;
}