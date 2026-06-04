namespace SweepsTracker.Core;

using System.ComponentModel.DataAnnotations;
using SQLite;
public class SweepsDatabase
{
    SQLiteAsyncConnection? database;
    private SQLiteAsyncConnection Database => database ?? throw new InvalidOperationException("Database has not been initialized. Call OpenAsync first.");

    public SweepsDatabase()
    {
        // Avoid blocking in the constructor; initialize lazily on first use.
    }

    public async Task OpenAsync()
    {
         if (database is not null)
            return;

        var connection = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await connection.CreateTableAsync<Game>();
        await connection.CreateTableAsync<Player>();
        await connection.CreateTableAsync<Round>();
        await connection.CreateTableAsync<RoundScore>();
        await connection.CreateTableAsync<Winner>();
        database = connection;
        Console.WriteLine($"database is stored at {Constants.DatabasePath}");
    }

    public async Task CloseAsync()
    {
        if (database is not null)
        {
            await database.CloseAsync();
        }
    }

    public async Task<Game?> CreateNewGameAsync(Game game, List<Player> players)
    {
        if(game.MaxScore < 150 || game.MaxScore > 500)
            return null;

        await OpenAsync();
        // date time record
        DateTime dateTime = DateTime.Now;
        game.StartDate = dateTime.ToString();

        // create the game record
        int createGame = await Database.InsertAsync(game);

        // set player GameID to the created game ID
        for(int i = 0; i < players.Count(); i++)
        {
            players[i].GameID = game.ID;
        }
        
        // create player records
        int createdPlayer = await Database.InsertAllAsync(players);

        if(createGame > 0 && createdPlayer > 0)
        {
            return game;
        }
        return null;
    }

    public async Task<string> MarkGameCompletedAsync(int gameID)
    {
        await OpenAsync();
        // get the selected game
        Game? game = await Database.FindAsync<Game>(gameID);
        
        if(game != null)
        {

            // get a list of all the players in game
            List<Player> players = await Database.Table<Player>().Where(p => p.GameID == gameID).ToListAsync();
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
            await Database.InsertAllAsync(winners);

            // set the end date
            game.EndDate = DateTime.Now.ToString();
            bool success = await Database.UpdateAsync(game) > 0;
            if(success)
                return "";
            else
                return "Error: failed to update game record";
        }
        return "Error: failed to find game record";
    }

    public async Task<Game?> AddRoundScoresAsync(int gameID, Dictionary<Player, int> roundScores)
    {
        if(roundScores.Count < 0)
            return null;
        
        // ensure that the players belong to the game session
        foreach(var entry in roundScores)
        {
            Player player = player = entry.Key;
            if(entry.Key.GameID != gameID)
                return null;
        }

        await OpenAsync();

        // check if the game is finished
        Game? foundGame = await Database.FindAsync<Game>(gameID);
        if(foundGame == null)
            return null;
        if(foundGame.EndDate != null)
            return null;

        // check if round already exists
        List<Round> roundsQuery = await Database.Table<Round>().Where(r => r.GameID == foundGame.ID).ToListAsync();
        Round? currentRound = null;
        if(roundsQuery.Count == 0)
        {
            // create the round
            Round round = new Round();
            round.GameID = foundGame.ID;
            round.RoundNumber = 1;
            await Database.InsertAsync(round);
            currentRound = round;
        }
        else
        {
            // rounds exist
            // find the highest round
            int highestRound = 0;
            foreach(Round round in roundsQuery)
            {
                if(round.RoundNumber > highestRound)
                {
                    highestRound = round.RoundNumber;
                }
            }

            // create the new round
            Round newRound = new Round();
            newRound.GameID = foundGame.ID;
            newRound.RoundNumber = highestRound + 1;
            await Database.InsertAsync(newRound);
            currentRound = newRound;
        }

        bool gameShouldEnd = false;
        // add round scores
        foreach(KeyValuePair<Player, int> entry in roundScores)
        {
            // update round score
            RoundScore roundScore = new RoundScore();
            roundScore.RoundID = currentRound.ID;
            roundScore.PlayerID = entry.Key.ID;
            roundScore.Score = entry.Value;
            await Database.InsertAsync(roundScore);

            // update player total score
            entry.Key.TotalScore += roundScore.Score;
            await Database.UpdateAsync(entry.Key);

            // check if player has won
            if(entry.Key.TotalScore >= foundGame.MaxScore)
            {
                gameShouldEnd = true;
            }
        }
        
        if(gameShouldEnd)
            await MarkGameCompletedAsync(foundGame.ID);

        return await GetGameFromGameIdAsync(foundGame.ID);
    }

    public async Task<List<Game>> GetActiveGamesAsync()
    {
        await OpenAsync();

        List<Game> activeGames = await Database.Table<Game>().Where(g => g.EndDate == null).ToListAsync();
        return activeGames;
    }

    public async Task<List<Game>> GetHistoricalGamesAsync()
    {
        await OpenAsync();

        List<Game> historicalGames = await Database.Table<Game>().Where(g => g.EndDate != null).ToListAsync();
        return historicalGames;
    }

    public async Task<Game> GetGameFromGameIdAsync(int gameID)
    {
        await OpenAsync();
        return await Database.FindAsync<Game>(gameID);
    }

    public async Task<GameInfo?> GetGameInfoAsync(int gameID)
    {
        Game game = await GetGameFromGameIdAsync(gameID);
        if(game == null)
            return null;

        await OpenAsync();

        GameInfo gameInfo = new GameInfo();
        gameInfo.Game = game;
        
        List<Player> players = await Database.Table<Player>().Where(p => p.GameID == gameID).ToListAsync();
        List<Round> rounds = await Database.Table<Round>().Where(r => r.GameID == gameID).ToListAsync();

        // sort the rounds
        bool swapped;
        for(int i = 0; i < rounds.Count() - 1; i++)
        {
            swapped = false;
            Round temp;
            for(int j = 0; j < rounds.Count() - i - 1; j++)
            {
                if (rounds[j].RoundNumber > rounds[j + 1].RoundNumber) {
                    
                    temp = rounds[j];
                    rounds[j] = rounds[j + 1];
                    rounds[j + 1] = temp;
                    swapped = true;
                }

                if (swapped == false)
                    break;
            }
        }

        gameInfo.RoundCount = rounds.Count();

        // store player objects
        foreach(Player player in players)
        {
            gameInfo.Info.Add(player, new List<int>());
        }

        // get a list of round scores per player
        foreach(Round round in rounds)
        {
            foreach(var entry in gameInfo.Info)
            {
                List<RoundScore> roundScores = await Database.Table<RoundScore>().Where(rs => rs.RoundID == round.ID && rs.PlayerID == entry.Key.ID).ToListAsync();
                entry.Value.Add(roundScores[0].Score);
            }
        }

        // get a list of the winning players
        List<Winner> winners = await Database.Table<Winner>().Where(winner => winner.GameID == game.ID).ToListAsync();

        foreach(Winner winner in winners)
        {
            Player player = await Database.FindAsync<Player>(winner.PlayerWinnerID);
            gameInfo.WinningPlayers.Add(player);
        }
        
        return gameInfo;
    }
}