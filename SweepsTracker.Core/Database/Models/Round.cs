namespace SweepsTracker.Core;

using System.ComponentModel.DataAnnotations.Schema;
using SQLite;

public class Round
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public int GameID { get; set; }

    public int RoundNumber { get; set; }
}