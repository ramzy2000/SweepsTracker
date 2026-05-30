namespace SweepsTracker.Test;

using Microsoft.VisualBasic;
using SweepsTracker.Core;

public class Database
{
    [Fact]
    public async Task TestCreateNewGame()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.Name = "My Game";
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        bool success = await sweepsDatabase.CreateNewGame(game, players);
        await sweepsDatabase.CloseAsync();
        Assert.True(success);
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestMarkGameCompleted()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.Name = "My Game";
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";
        player.TotalScore = 150;

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        bool success = await sweepsDatabase.CreateNewGame(game, players);
        Assert.True(await sweepsDatabase.MarkGameCompleted(1) == ""); // mark existing game completed

        Assert.False(await sweepsDatabase.MarkGameCompleted(5) == ""); // try mark non existant game completed
        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestMarkGameCompletedNoMaxScore()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.Name = "My Game";
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        bool success = await sweepsDatabase.CreateNewGame(game, players);
        Assert.False(await sweepsDatabase.MarkGameCompleted(1) == ""); // mark existing game completed
        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestMarkAddRoundScores()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.Name = "My Game";
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        bool success = await sweepsDatabase.CreateNewGame(game, players);

        // add some score

        Dictionary<Player, int> roundScores = new Dictionary<Player, int>();
        roundScores.Add(player, 30);
        roundScores.Add(player1, 50);

        Assert.Equal("", await sweepsDatabase.AddRoundScores(game.ID, roundScores));

        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }

    [Fact]
    public async Task TestMarkAddRoundScoresWinners()
    {
        SweepsDatabase sweepsDatabase = new SweepsDatabase();
        Game game = new Game();
        game.Name = "My Game";
        game.MaxScore = 150;

        Player player = new Player();
        player.Name = "Casey";

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        bool success = await sweepsDatabase.CreateNewGame(game, players);

        // add some score

        Dictionary<Player, int> roundScores = new Dictionary<Player, int>();
        roundScores.Add(player, 30);
        roundScores.Add(player1, 50);

        Assert.Equal("", await sweepsDatabase.AddRoundScores(game.ID, roundScores));

        Assert.Equal("", await sweepsDatabase.AddRoundScores(game.ID, roundScores));

        Assert.Equal("", await sweepsDatabase.AddRoundScores(game.ID, roundScores));

        Assert.False(await sweepsDatabase.AddRoundScores(game.ID, roundScores) == ""); // should fail and game should be over

        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }
}
