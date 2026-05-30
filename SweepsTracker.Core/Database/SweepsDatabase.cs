namespace SweepsTracker.Core;

using System.ComponentModel.DataAnnotations;
using SQLite;
public class SweepsDatabase
{
    SQLiteAsyncConnection database;

    public SweepsDatabase()
    {
        OpenAsync().Wait();
    }

    public async Task OpenAsync()
    {
         if (database is not null)
            return;

        database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await database.CreateTableAsync<Game>();
        await database.CreateTableAsync<Player>();
        await database.CreateTableAsync<Round>();
        await database.CreateTableAsync<RoundScore>();
        await database.CreateTableAsync<Winner>();
        Console.WriteLine($"database is stored at {Constants.DatabasePath}");
    }

    public async Task CloseAsync()
    {
        await database.CloseAsync();
    }

    public async Task<bool> CreateNewGame(Game game, List<Player> players)
    {
        if(game.MaxScore < 150 || game.MaxScore > 500)
            return false;

        await OpenAsync();
        // date time record
        DateTime dateTime = DateTime.Now;
        game.StartDate = dateTime.ToString();

        // create the game record
        int createGame = await database.InsertAsync(game);

        // set player GameID to the created game ID
        for(int i = 0; i < players.Count(); i++)
        {
            players[i].GameID = game.ID;
        }
        
        // create player records
        int createdPlayer = await database.InsertAllAsync(players);

        if(createGame > 0 && createdPlayer > 0)
        {
            return true;
        }
        return false;
    }

    public async Task<string> MarkGameCompleted(int gameID)
    {
        await OpenAsync();
        // get the selected game
        Game? game = await database.FindAsync<Game>(gameID);
        
        if(game != null)
        {

            // get a list of all the players in game
            List<Player> players = await database.Table<Player>().Where(p => p.GameID == gameID).ToListAsync();
            Player lowestScorePlayer = players[0];

            // check if the players have met the max score at least on of them
            bool isMaxScore = false;
            foreach(Player player in players)
            {
                if(player.TotalScore >= game.MaxScore)
                {
                    isMaxScore = true;
                }
            }

            if(!isMaxScore)
                return "Error: no player met max score.";

            // get who got the lowest score player
            foreach(Player player in players)
            {
                if(player.TotalScore < lowestScorePlayer.TotalScore)
                {
                    lowestScorePlayer = player;
                }
            }
            
            // check if there are more than one winner
            List<Player> winnerPlayers = new List<Player>();
            foreach(Player player in players)
            {
                if(lowestScorePlayer.TotalScore == player.TotalScore)
                {
                    winnerPlayers.Add(player);
                }
            }

            // if no winner do add to database
            if(winnerPlayers.Count == 0)
                return "Error: no winners found";

            
            // create a winner records
            List<Winner> winners = new List<Winner>();
            foreach(Player player in winnerPlayers)
            {
                Winner winner = new Winner();
                winner.GameID = game.ID;
                winner.PlayerWinnerID = player.ID;
                winners.Add(winner);
            }

            // create the winner records
            await database.InsertAllAsync(winners);

            // set the end date
            game.EndDate = DateTime.Now.ToString();
            bool success = await database.UpdateAsync(game) > 0;
            if(success)
                return "";
            else
                return "Error: failed to update game record";
        }
        return "Error: failed to find game record";
    }
}