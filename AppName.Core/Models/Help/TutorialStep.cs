namespace AppName.Core.Models.Help;

public sealed class TutorialStep
{
    public int StepNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Tip { get; set; } = string.Empty;

    public string? ImagePath { get; set; }
}
