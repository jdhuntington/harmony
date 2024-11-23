namespace Harmony.Lib.Models;

public class Team
{
    public required string Name { get; init; }
    public int? ByeRound { get; private set; }
    public bool HadBye => ByeRound != null;

    public int AffRounds { get; set; }
    public int NegRounds { get; set; }

    public int Losses { get; set; }
    public int Wins { get; set; }

    public void RecordBye(int round)
    {
        ByeRound = round;
    }

    public void RecordAff(int roundNumber)
    {
        AffRounds++;
        CheckRoundBalance();
    }

    public void RecordNeg(int roundNumber)
    {
        NegRounds++;
        CheckRoundBalance();
    }

    private void CheckRoundBalance()
    {
        if (Math.Abs(AffRounds - NegRounds) > 1) {
            throw new ImbalancedRoundsException(this);
        }
    }
}