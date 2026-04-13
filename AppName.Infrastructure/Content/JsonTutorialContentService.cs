using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Help;
using System.Text.Json;

namespace AppName.Infrastructure.Content;

public sealed class JsonTutorialContentService : ITutorialContentService
{
    private readonly string _filePath;

    public JsonTutorialContentService(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "Content", "tutorial-content.json");
    }

    public async Task<IReadOnlyList<TutorialTopic>> GetTopicsAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        var topics = await JsonSerializer.DeserializeAsync<List<TutorialTopic>>(stream, cancellationToken: cancellationToken);

        return topics?
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Title)
            .ToList() ?? [];
    }
}
