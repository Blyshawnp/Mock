using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Enums;
using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;

namespace AppName.Core.Services.Rules;

public sealed class EvaluationRulesService : IEvaluationRulesService
{
    public EvaluationResult Evaluate(EvaluationSession session)
    {
        var calls = session.Calls.OrderBy(x => x.CallNumber).ToList();
        var showCall3 = ShouldShowCall3(calls);

        var visibleCalls = calls
            .Where(x => x.CallNumber != 3 || showCall3)
            .ToList();

        var hasFail = visibleCalls.Any(x => x.Outcome == CallOutcome.Fail);
        var hasNotScored = visibleCalls.Any(x => x.Outcome == CallOutcome.NotScored);

        var transferFollowUp = session.Transfers.Any(x => x.Outcome == TransferOutcome.Fail && x.FollowUpRequired);

        return new EvaluationResult
        {
            ShowCall3 = showCall3,
            IsComplete = !hasNotScored,
            IsPassing = !hasFail && !hasNotScored,
            RequiresSupervisorFollowUp = transferFollowUp,
            BlockingIssues = hasNotScored ? ["All visible calls must be scored before review."] : [],
            InformationalMessages = showCall3
                ? ["Call 3 is required because Call 1 and Call 2 did not both pass."]
                : ["Call 3 is hidden because Call 1 and Call 2 both passed."]
        };
    }

    public bool ShouldShowCall3(IReadOnlyList<CallRecord> calls)
    {
        var call1 = calls.FirstOrDefault(x => x.CallNumber == 1);
        var call2 = calls.FirstOrDefault(x => x.CallNumber == 2);

        if (call1 is null || call2 is null)
        {
            return true;
        }

        return !(call1.Outcome == CallOutcome.Pass && call2.Outcome == CallOutcome.Pass);
    }
}
