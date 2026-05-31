namespace SweepsTracker.Core;
public class ActiveGameInfo
{
    public string GameName { get; set; }

    public List<string> PlayerNames { get; set; }

    public List<RoundInfo> RoundInfos { get; set; }

    public List<int> PlayerScoreTotals { get; set; }
}