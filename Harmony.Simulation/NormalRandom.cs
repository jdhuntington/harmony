namespace Harmony.Simulation;

public interface IRandomDouble
{
    /// <summary>
    ///     Returns a randomly generated double.
    /// </summary>
    double NextDouble();
}

/// <summary>
///     Generates random numbers following a normal (Gaussian) distribution.
/// </summary>
public class NormalRandom : IRandomDouble
{
    private readonly double _mean;
    private readonly double _stdDev;
    private readonly Random _random;

    // These are used to store a second normally distributed sample,
    // since the Box-Muller transform generates two numbers at once.
    private bool _hasSpare;
    private double _spare;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NormalRandom" /> class.
    /// </summary>
    /// <param name="mean">
    ///     The mean (expected value) of the distribution. It is recommended to be between 0 and 1.
    /// </param>
    /// <param name="stdDev">
    ///     The standard deviation of the distribution.
    ///     A small value (e.g., 0.05) produces values that are very close to the mean, while a larger value (e.g., 0.2)
    ///     produces values with a wider spread.
    /// </param>
    /// <param name="seed">
    ///     (Optional) An integer seed for the random number generator.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when the mean is not between 0 and 1 or when stdDev is not positive.
    /// </exception>
    public NormalRandom(double mean, double stdDev, int? seed = null)
    {
        if (mean < 0 || mean > 1)
            throw new ArgumentOutOfRangeException(nameof(mean), "Mean should be between 0 and 1.");
        if (stdDev <= 0)
            throw new ArgumentOutOfRangeException(nameof(stdDev), "Standard deviation should be positive.");

        _mean = mean;
        _stdDev = stdDev;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    ///     Generates a normally distributed random number.
    ///     <para>
    ///         The normal distribution is unbounded. Although the mean is provided between 0 and 1,
    ///         the generated value might be less than 0 or greater than 1.
    ///     </para>
    ///     <para>
    ///         Standard deviation effects:
    ///         - A small standard deviation (e.g., 0.05) means that roughly 68% of values will lie within
    ///         the range (mean − 0.05, mean + 0.05) and about 95% within (mean − 0.1, mean + 0.1).
    ///         - A larger standard deviation (e.g., 0.2) produces a wider spread of values.
    ///     </para>
    /// </summary>
    /// <returns>A normally distributed double value.</returns>
    public double NextDouble()
    {
        // If we have a spare sample from a previous call, use it.
        if (_hasSpare)
        {
            _hasSpare = false;
            return _mean + _stdDev * _spare;
        }

        // Use the Box-Muller transform (the polar form) to generate two independent normal samples.
        double u, v, s;
        do
        {
            // Generate u and v in the range [-1, 1].
            u = _random.NextDouble() * 2.0 - 1.0;
            v = _random.NextDouble() * 2.0 - 1.0;
            s = u * u + v * v;
        } while (s >= 1.0 || s == 0.0);

        // The transformation factor.
        s = Math.Sqrt(-2.0 * Math.Log(s) / s);

        // Store one of the generated values for the next call.
        _spare = v * s;
        _hasSpare = true;

        return _mean + _stdDev * (u * s);
    }
}