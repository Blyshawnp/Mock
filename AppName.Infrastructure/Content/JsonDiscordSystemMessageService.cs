using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
using System.Text.Json;

namespace AppName.Infrastructure.Content;

public sealed class JsonDiscordSystemMessageService : IDiscordSystemMessageService
{
    private readonly string _filePath;

    public JsonDiscordSystemMessageService(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "Content", "discord-system-messages.json");
    }

    public async Task<IReadOnlyList<DiscordSystemMessage>> GetMessagesAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        var messages = await JsonSerializer.DeserializeAsync<List<DiscordSystemMessage>>(stream, cancellationToken: cancellationToken);

        return messages ?? [];
    }
}
