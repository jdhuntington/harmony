namespace Harmony.Simulation.Tests;

public class PredictableRandom(double value) : IRandomDouble
{
    public double NextDouble()
    {
        return value;
    }
}