using AppName.Core.Models.Sessions;

namespace AppName.Core.Interfaces.Services;

public interface ISummaryService
{
    Task<string> GenerateCoachingSummaryAsync(EvaluationSession session, CancellationToken cancellationToken = default);

    Task<string> GenerateFailSummaryAsync(EvaluationSession session, CancellationToken cancellationToken = default);
}
