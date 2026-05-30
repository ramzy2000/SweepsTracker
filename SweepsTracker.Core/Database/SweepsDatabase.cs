namespace SweepsTracker.Core;

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

    public async Task<bool> MarkGameCompleted(int gameID)
    {
        await OpenAsync();
        // get the selected game
        var gameQuery = database.Table<Game>().Where(g => g.ID == gameID);

        bool isFound = await gameQuery.CountAsync() > 0;
        
        if(gameQuery != null && isFound)
        {
            Game game = await gameQuery.FirstAsync();

            game.EndDate = DateTime.Now.ToString();
            return await database.UpdateAsync(game) > 0;
        }
        return false;
    }
}