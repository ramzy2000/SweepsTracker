using SweepsTracker.Core;
using System;
using System.Collections.Generic;
using System.Text;

public class GameDisplayTable
{
    public Game Game { get; set; }

    public List<string> names = new List<string>();

    public List<List<int>> displayScores = new List<List<int>>();

    public List<Player> WinningPlayers = new List<Player>();

    public int RoundCount = 0;
}
