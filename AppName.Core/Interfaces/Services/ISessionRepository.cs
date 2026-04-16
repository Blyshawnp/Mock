using AppName.Core.Models.Sessions;

namespace AppName.Core.Interfaces.Services;

public interface ISessionRepository
{
    Task SaveAsync(EvaluationSession session, CancellationToken cancellationToken = default);

    Task<EvaluationSession?> LoadLatestAsync(CancellationToken cancellationToken = default);

    Task<EvaluationSession?> LoadAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
