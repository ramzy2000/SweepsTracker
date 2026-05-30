namespace SweepsTracker.Test;

using Microsoft.VisualBasic;
using SweepsTracker.Core;

public class Database : IDisposable
{
    [Fact]
    public async Task TestCreateNewGame()
    {
        // create the database
        {
            SweepsDatabase sweepsDatabase = new SweepsDatabase();
            Game game = new Game();
            game.Name = "My Game";

            Player player = new Player();
            player.Name = "Casey";

            Player player1 = new Player();
            player1.Name = "Justine";

            List<Player> players = new List<Player>();
            players.Add(player);
            players.Add(player1);
            bool success =  await sweepsDatabase.CreateNewGame(game, players);
            await sweepsDatabase.CloseAsync();
            Assert.True(success);
        }
    }

    public void Dispose()
    {
        FileSystem.Kill(SweepsTracker.Core.Constants.DatabasePath);
    }
}
