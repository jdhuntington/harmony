using System.Data;

namespace Harmony.Lib.Models;

public class Matchup
{
    public required Team Aff { get; init; }
    public Team? Neg { get; init; }
    private Round? Round { get; set; }

    public bool IsBye => Neg == null;

    public void Validate()
    {
        if (IsBye && Aff.HadBye) throw new TooManyByesException(Aff);
    }

    public bool Contains(Team team)
    {
        return Aff == team || Neg == team;
    }

    public void Record(Round recordedRound)
    {
        Round = recordedRound;
        if (IsBye)
        {
            Aff.RecordBye(Round.Number);
        }
        else
        {
            Aff.RecordAff(Round.Number);
            if (Neg == null) throw new NoNullAllowedException("Neg should not be null");
            Neg.RecordNeg(Round.Number);
        }
    }

    public override string ToString()
    {
        return IsBye ? $"{Aff.Name} has a bye" : $"{Aff} vs {Neg}";
    }
}