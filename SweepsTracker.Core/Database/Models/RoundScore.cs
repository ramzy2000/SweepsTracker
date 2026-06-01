namespace SweepsTracker.Core;

using System.ComponentModel.DataAnnotations.Schema;
using SQLite;

public class RoundScore
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public int RoundID { get; set; }

    public int PlayerID { get; set; }

    public int Score { get; set; }
}