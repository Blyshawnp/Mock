namespace AppName.Core.Models.Review;

public sealed class ReviewData
{
    public string CoachingSummary { get; set; } = string.Empty;

    public string FailSummary { get; set; } = string.Empty;

    public bool CoachingSummaryGenerated { get; set; }

    public bool FailSummaryGenerated { get; set; }

    public bool ReadyForSubmit { get; set; }

    public bool SubmitConfirmed { get; set; }
}
