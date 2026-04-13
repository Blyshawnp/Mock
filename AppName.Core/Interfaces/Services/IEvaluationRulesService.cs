using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;

namespace AppName.Core.Interfaces.Services;

public interface IEvaluationRulesService
{
    EvaluationResult Evaluate(EvaluationSession session);

    bool ShouldShowCall3(IReadOnlyList<CallRecord> calls);
}
