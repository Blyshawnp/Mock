using AppName.Core.Models.Enums;

namespace AppName.Core.Models.Sessions;

public sealed class TransferRecord
{
    public int AttemptNumber { get; set; }

    public TransferOutcome Outcome { get; set; } = TransferOutcome.NotAttempted;

    public string? Reason { get; set; }

    public string? Notes { get; set; }

    public bool FollowUpRequired { get; set; }

    public DateTimeOffset? FollowUpDate { get; set; }
}
