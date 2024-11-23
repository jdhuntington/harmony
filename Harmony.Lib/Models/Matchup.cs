using Harmony.Lib.Exceptions;

namespace Harmony.Lib.Models;

public class Matchup
{
    public required Team Aff { get; set; }
    public Team? Neg { get; set; }

    public bool IsBye => Neg == null;

    public void Validate()
    {
        if (IsBye && Aff.HadBye)
        {
            throw new TooManyByesException("${Aff.Name} has already had a bye.");
        }
    }

    public void Record()
    {
        if (IsBye)
        {
            Aff.RecordBye();
        }        
    }
}
