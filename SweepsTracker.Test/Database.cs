namespace SweepsTracker.Test;

using Microsoft.VisualBasic;
using SweepsTracker.Core;

public class Database
{
    [Fact]
    public async Task TestCreateNewGameAsync()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        Game? successGameObject = await sweepsDatabase.CreateNewGameAsync(game, players);
        Assert.False(successGameObject == null);
        Assert.False(successGameObject.ID == 0);
        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestMarkGameCompletedAsync()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";
        player.TotalScore = 150;

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        await sweepsDatabase.CreateNewGameAsync(game, players);
        Assert.True(await sweepsDatabase.MarkGameCompletedAsync(1) == ""); // mark existing game completed

        Assert.False(await sweepsDatabase.MarkGameCompletedAsync(5) == ""); // try mark non existant game completed
        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestMarkGameCompletedNoMaxScoreAsync()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        await sweepsDatabase.CreateNewGameAsync(game, players);
        Assert.False(await sweepsDatabase.MarkGameCompletedAsync(1) == ""); // mark existing game completed
        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestMarkAddRoundScoresAsync()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        await sweepsDatabase.CreateNewGameAsync(game, players);

        // add some score

        Dictionary<Player, int> roundScores = new Dictionary<Player, int>();
        roundScores.Add(player, 30);
        roundScores.Add(player1, 50);

        Assert.False(await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores) == null);

        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestMarkAddRoundScoresWinnersAsync()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        await sweepsDatabase.CreateNewGameAsync(game, players);

        // add some score

        Dictionary<Player, int> roundScores = new Dictionary<Player, int>();
        roundScores.Add(player, 30);
        roundScores.Add(player1, 50);

        Assert.False(await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores) == null);

        Assert.False(await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores) == null);

        Assert.False(await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores) == null);

        Assert.True(await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores) == null); // should fail and game should be over

        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestGetActiveGamesAsync()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);

        // create two games
        await sweepsDatabase.CreateNewGameAsync(game, players);

        await sweepsDatabase.CreateNewGameAsync(game, players);

        List<Game> activeGames = await sweepsDatabase.GetActiveGamesAsync();
        foreach (Game activeGame in activeGames)
        {
            Assert.True(activeGame.EndDate == null); // make sure no games are historical
        }

        Assert.Equal(2, activeGames.Count());

        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestGetHistoricalGamesAsync()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();

        for (int i = 0; i < 2; i++)
        {
            Game game = new Game();
            game.MaxScore = 150;

            Player player = new Player();
            player.Name = "Casey";

            Player player1 = new Player();
            player1.Name = "Justine";

            List<Player> players = new List<Player>();
            players.Add(player);
            players.Add(player1);
            await sweepsDatabase.CreateNewGameAsync(game, players);

            // add some score

            Dictionary<Player, int> roundScores = new Dictionary<Player, int>();
            roundScores.Add(player, 30);
            roundScores.Add(player1, 50);

            await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores);

            await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores);

            await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores);

            await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores); // should fail and game should be over
        }

        // get historical games
        List<Game> historicalGames = await sweepsDatabase.GetHistoricalGamesAsync();

        foreach (Game historicalGame in historicalGames)
        {
            Assert.True(historicalGame.EndDate != null);
        }

        Assert.Equal(2, historicalGames.Count());

        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestGetGameFromGameIdAsync()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();

        Game game = new Game();
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        await sweepsDatabase.CreateNewGameAsync(game, players);

        // test the method
        Game testGameObject = await sweepsDatabase.GetGameFromGameIdAsync(1);
        Assert.True(testGameObject != null);

        Game testGameObject2 = await sweepsDatabase.GetGameFromGameIdAsync(2);
        Assert.True(testGameObject2 == null);

        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestGetGameInfoAsync()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();

        Game game = new Game();
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        await sweepsDatabase.CreateNewGameAsync(game, players);

        Dictionary<Player, int> roundScores = new Dictionary<Player, int>();
        roundScores.Add(player, 30);
        roundScores.Add(player1, 50);

        await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores);

        await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores);

        await sweepsDatabase.AddRoundScoresAsync(game.ID, roundScores);

        // test the method
        Game testGameObject = await sweepsDatabase.GetGameFromGameIdAsync(1);


        // test the DTO
        GameInfo? gameInfo = await sweepsDatabase.GetGameInfoAsync(1);
        Assert.False(gameInfo == null);

        Assert.True(gameInfo.Info.Count() == 2);

        foreach(var entry in gameInfo.Info)
        {
            Assert.False(entry.Key.Name == "" || entry.Key.Name == null);
            Assert.True(entry.Value.Count() == 3);
        }

        Assert.True(gameInfo.WinningPlayers.Count() == 1);

        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }
}
