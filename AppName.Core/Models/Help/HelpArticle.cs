namespace AppName.Core.Models.Help;

public sealed class HelpArticle
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];

    public string RelatedScreen { get; set; } = string.Empty;
}
