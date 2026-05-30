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

        Player player1 = new Player();
        player1.Name = "Justine";

        List<Player> players = new List<Player>();
        players.Add(player);
        players.Add(player1);
        bool success = await sweepsDatabase.CreateNewGame(game, players);
        Assert.True(await sweepsDatabase.MarkGameCompleted(1)); // mark existing game completed

        Assert.False(await sweepsDatabase.MarkGameCompleted(5)); // try mark non existant game completed
        await sweepsDatabase.CloseAsync();
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }
}
