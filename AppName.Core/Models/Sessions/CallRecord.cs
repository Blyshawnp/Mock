using AppName.Core.Models.Enums;

namespace AppName.Core.Models.Sessions;

public sealed class CallRecord
{
    public int CallNumber { get; set; }

    public CallOutcome Outcome { get; set; } = CallOutcome.NotScored;

    public List<string> CoachingSelections { get; set; } = [];

    public List<string> FailSelections { get; set; } = [];

    public string? OtherCoachingText { get; set; }

    public string? OtherFailText { get; set; }

    public string? Notes { get; set; }

    public bool IsVisible { get; set; } = true;

    public bool IsComplete { get; set; }
}
