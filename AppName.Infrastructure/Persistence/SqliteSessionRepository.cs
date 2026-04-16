using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Sessions;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace AppName.Infrastructure.Persistence;

public sealed class SqliteSessionRepository : ISessionRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private readonly DatabaseInitializer _initializer;

    public SqliteSessionRepository(DatabaseInitializer initializer)
    {
        _initializer = initializer;
    }

    public async Task SaveAsync(EvaluationSession session, CancellationToken cancellationToken = default)
    {
        await _initializer.InitializeAsync(cancellationToken);

        await using var connection = new SqliteConnection(_initializer.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO SessionSnapshots (SessionId, CreatedAt, UpdatedAt, SessionJson)
            VALUES ($sessionId, $createdAt, $updatedAt, $sessionJson)
            ON CONFLICT(SessionId) DO UPDATE SET
                UpdatedAt = excluded.UpdatedAt,
                SessionJson = excluded.SessionJson;
            """;

        command.Parameters.AddWithValue("$sessionId", session.SessionId.ToString("D"));
        command.Parameters.AddWithValue("$createdAt", session.CreatedAt.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$updatedAt", session.UpdatedAt.UtcDateTime.ToString("O"));
        command.Parameters.AddWithValue("$sessionJson", JsonSerializer.Serialize(session, JsonOptions));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<EvaluationSession?> LoadLatestAsync(CancellationToken cancellationToken = default)
    {
        await _initializer.InitializeAsync(cancellationToken);

        await using var connection = new SqliteConnection(_initializer.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT SessionJson
            FROM SessionSnapshots
            ORDER BY UpdatedAt DESC
            LIMIT 1;
            """;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Deserialize(result as string);
    }

    public async Task<EvaluationSession?> LoadAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await _initializer.InitializeAsync(cancellationToken);

        await using var connection = new SqliteConnection(_initializer.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT SessionJson
            FROM SessionSnapshots
            WHERE SessionId = $sessionId
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("$sessionId", sessionId.ToString("D"));
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Deserialize(result as string);
    }

    private static EvaluationSession? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<EvaluationSession>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
