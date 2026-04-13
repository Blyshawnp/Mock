using AppName.Core.Interfaces.Services;

namespace AppName.Infrastructure.Logging;

public sealed class FileLogService : ILogService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public FileLogService(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "Logs", "app.log");

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public Task LogInfoAsync(string message, CancellationToken cancellationToken = default)
    {
        return WriteLineAsync("INFO", message, cancellationToken);
    }

    public Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        var fullMessage = exception is null
            ? message
            : $"{message} | {exception.GetType().Name}: {exception.Message}";

        return WriteLineAsync("ERROR", fullMessage, cancellationToken);
    }

    private async Task WriteLineAsync(string level, string message, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var line = $"{DateTimeOffset.UtcNow:O} [{level}] {message}{Environment.NewLine}";
            try
            {
                await File.AppendAllTextAsync(_filePath, line, cancellationToken);
            }
            catch
            {
                // Logging failures must not crash application flow.
            }
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
