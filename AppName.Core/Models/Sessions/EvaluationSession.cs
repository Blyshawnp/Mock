using AppName.Core.Models.Common;
using AppName.Core.Models.Enums;
using AppName.Core.Models.Review;

namespace AppName.Core.Models.Sessions;

public sealed class EvaluationSession
{
    public Guid SessionId { get; set; } = Guid.NewGuid();

    public string EvaluatorName { get; set; } = string.Empty;

    public string CandidateName { get; set; } = string.Empty;

    public bool FinalAttempt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public SessionStatus Status { get; set; } = SessionStatus.Draft;

    public string CurrentScreenKey { get; set; } = "Dashboard";

    public List<CallRecord> Calls { get; set; } =
    [
        new() { CallNumber = 1, DonorScenario = DonorScenario.NewDonor, IsVisible = true },
        new() { CallNumber = 2, DonorScenario = DonorScenario.ExistingMember, IsVisible = true },
        new() { CallNumber = 3, DonorScenario = DonorScenario.IncreaseSustaining, IsVisible = true }
    ];

    public List<TransferRecord> Transfers { get; set; } =
    [
        new() { AttemptNumber = 1 },
        new() { AttemptNumber = 2 }
    ];

    public ReviewData Review { get; set; } = new();

    public EvaluationResult Result { get; set; } = new();

    public List<AppWarning> Warnings { get; set; } = [];

    public double ProgressPercent { get; set; }
}
