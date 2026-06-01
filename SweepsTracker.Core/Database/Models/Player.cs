namespace SweepsTracker.Core;

using System.ComponentModel.DataAnnotations.Schema;
using SQLite;

public class Player
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public int GameID { get; set; }

    public string Name { get; set; }

    public int TotalScore { get; set; }
}