namespace Harmony.Lib.Models;

public class Team
{
    private int _affRounds;
    private int _negRounds;
    public required string Name { get; init; }
    public int? ByeRound { get; private set; }
    public bool HadBye => ByeRound != null;

    public void RecordBye(int round)
    {
        ByeRound = round;
    }

    public void RecordAff(int roundNumber)
    {
        _affRounds++;
    }

    public void RecordNeg(int roundNumber)
    {
        _negRounds++;
    }
}