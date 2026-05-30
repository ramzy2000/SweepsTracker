namespace SweepsTracker.Core;

using System.ComponentModel.DataAnnotations.Schema;
using SQLite;

public class Game
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string Name { get; set; }
    public int MaxScore { get; set; }
    public string StartDate { get; set; }

    public string EndDate { get; set; }
    public int winner_player_id { get; set; }
}