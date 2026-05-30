using SQLite;
public class SweepsDatabase
{
    SQLiteAsyncConnection database;

    public SweepsDatabase()
    {
        if (database is not null)
            return;

        database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        database.CreateTableAsync<Game>().Wait();
        database.CreateTableAsync<Player>().Wait();
        database.CreateTableAsync<Round>().Wait();
        database.CreateTableAsync<RoundScore>().Wait();
    }
}