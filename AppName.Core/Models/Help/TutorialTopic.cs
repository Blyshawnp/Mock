namespace AppName.Core.Models.Help;

public sealed class TutorialTopic
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ScreenKey { get; set; } = string.Empty;

    public List<TutorialStep> Steps { get; set; } = [];

    public List<string> Tags { get; set; } = [];
}
