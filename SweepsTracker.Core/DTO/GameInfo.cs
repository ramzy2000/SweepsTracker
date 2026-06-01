namespace SweepsTracker.Core;

public class GameInfo
{
    public Game Game { get; set; }

    public Dictionary<Player, List<int>> Info = new Dictionary<Player, List<int>>();

    public List<Player> WinningPlayers = new List<Player>();
}
