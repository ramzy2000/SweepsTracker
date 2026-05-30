namespace SweepsTracker.Test;
using SweepsTracker.Core;

public class Database
{
    [Fact]
    public async Task TestCreateNewGame()
    {
        // create the database
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
        await sweepsDatabase.CreateNewGame(game, players);
    }
}
