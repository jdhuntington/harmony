namespace Harmony.Lib.Models;

public class Team
{
    public required string Name { get; set; }
    public bool HadBye { get; private set; }

    public void RecordBye()
    {
        HadBye = true;
    }
}