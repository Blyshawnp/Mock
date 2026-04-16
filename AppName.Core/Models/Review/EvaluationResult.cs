namespace AppName.Core.Models.Review;

public sealed class EvaluationResult
{
    public bool IsPassing { get; set; }

    public bool IsComplete { get; set; }

    public bool ShowCall3 { get; set; } = true;

    public bool RequiresSupervisorFollowUp { get; set; }

    public string ReasonForFail { get; set; } = string.Empty;

    public List<string> BlockingIssues { get; set; } = [];

    public List<string> InformationalMessages { get; set; } = [];
}
