using Microsoft.Data.Sqlite;

namespace AppName.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _isInitialized;

    public DatabaseInitializer(string? databasePath = null)
    {
        var dbPath = databasePath
            ?? Path.Combine(AppContext.BaseDirectory, "Data", "appname.db");

        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = $"Data Source={dbPath}";
    }

    public string ConnectionString => _connectionString;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
            {
                return;
            }

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var command = connection.CreateCommand();
            command.CommandText =
                """
                CREATE TABLE IF NOT EXISTS SessionSnapshots (
                    SessionId TEXT PRIMARY KEY,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    SessionJson TEXT NOT NULL
                );
                """;

            await command.ExecuteNonQueryAsync(cancellationToken);
            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }
}
