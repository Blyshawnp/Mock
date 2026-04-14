namespace AppName.Core.Models.Config;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Light";

    public bool SoundsEnabled { get; set; } = true;

    public string DefaultSaveFolder { get; set; } = string.Empty;

    public int AutosaveMinutes { get; set; } = 5;

    public bool FirstRunComplete { get; set; }

    public bool TutorialCompleted { get; set; }

    public bool BeginnerModeEnabled { get; set; } = true;
}
