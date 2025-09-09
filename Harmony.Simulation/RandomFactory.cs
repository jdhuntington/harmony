namespace Harmony.Simulation;

public interface IRandomFactory
{
    IRandomDouble BuildRandom(double mean, double stdDev);
}
public class RandomFactory : IRandomFactory
{
    public IRandomDouble BuildRandom(double mean, double stdDev)
    {
        return new NormalRandom(mean, stdDev);
    }
}