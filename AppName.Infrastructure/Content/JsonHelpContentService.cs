using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Help;
using System.Text.Json;

namespace AppName.Infrastructure.Content;

public sealed class JsonHelpContentService : IHelpContentService
{
    private readonly string _filePath;

    public JsonHelpContentService(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppContext.BaseDirectory, "Content", "help-content.json");
    }

    public async Task<IReadOnlyList<HelpArticle>> GetArticlesAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        var articles = await JsonSerializer.DeserializeAsync<List<HelpArticle>>(stream, cancellationToken: cancellationToken);

        return articles?
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Title)
            .ToList() ?? [];
    }
}
