using AppName.Core.Models.Common;

namespace AppName.Core.Interfaces.Services;

public interface IDiscordSystemMessageService
{
    Task<IReadOnlyList<DiscordSystemMessage>> GetMessagesAsync(CancellationToken cancellationToken = default);
}
