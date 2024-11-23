namespace Harmony.Lib.Models;

public class Team
{
    private int _affRounds;
    private int _negRounds;
    public required string Name { get; init; }
    public int? ByeRound { get; private set; }
    public bool HadBye => ByeRound != null;

    public int AffRounds => _affRounds;
    public int NegRounds => _negRounds;

    public void RecordBye(int round)
    {
        ByeRound = round;
    }

    public void RecordAff(int roundNumber)
    {
        _affRounds++;
        CheckRoundBalance();
    }

    public void RecordNeg(int roundNumber)
    {
        _negRounds++;
        CheckRoundBalance();
    }

    private void CheckRoundBalance()
    {
        if (Math.Abs(_affRounds - _negRounds) > 1) {
            throw new ImbalancedRoundsException(this);
        }
    }
}