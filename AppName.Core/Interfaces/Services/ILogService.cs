namespace AppName.Core.Interfaces.Services;

public interface ILogService
{
    Task LogInfoAsync(string message, CancellationToken cancellationToken = default);

    Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default);
}
