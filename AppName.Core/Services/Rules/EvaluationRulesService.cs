using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Enums;
using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;

namespace AppName.Core.Services.Rules;

public sealed class EvaluationRulesService : IEvaluationRulesService
{
    public EvaluationResult Evaluate(EvaluationSession session)
    {
        var orderedCalls = (session.Calls ?? []).OrderBy(x => x.CallNumber).ToList();
        var orderedTransfers = (session.Transfers ?? []).OrderBy(x => x.AttemptNumber).ToList();

        var showCall3 = ShouldShowCall3(orderedCalls);
        var visibleCalls = orderedCalls
            .Where(x => x.CallNumber != 3 || showCall3)
            .ToList();

        var isSupervisorOnlySession = IsSupervisorOnlySession(orderedCalls);

        var passedCalls = visibleCalls.Where(x => x.Outcome == CallOutcome.Pass).ToList();
        var callsComplete = visibleCalls.All(x => x.Outcome != CallOutcome.NotScored);
        var hasPassedNewMemberCall = passedCalls.Any(x => x.DonorScenario == DonorScenario.NewDonor);
        var hasPassedExistingMemberCall = passedCalls.Any(x => x.DonorScenario == DonorScenario.ExistingMember);
        var mockCallsPassed = passedCalls.Count >= 2 && hasPassedNewMemberCall && hasPassedExistingMemberCall;

        var evaluatedTransfers = GetTransfersToEvaluate(orderedTransfers);
        var transferPassCount = evaluatedTransfers.Count(x => x.Outcome == TransferOutcome.Pass);
        var transfersComplete = evaluatedTransfers.All(x => x.Outcome != TransferOutcome.NotAttempted);
        var transfersPassed = transferPassCount >= 1;

        var transferFollowUp = evaluatedTransfers.Any(x => x.Outcome == TransferOutcome.Fail && x.FollowUpRequired);

        var notEnoughTimeScenario = !isSupervisorOnlySession
                                    && callsComplete
                                    && mockCallsPassed
                                    && !transfersComplete;

        var isComplete = isSupervisorOnlySession
            ? transfersComplete
            : (callsComplete && transfersComplete) || notEnoughTimeScenario;

        var isPassing = isSupervisorOnlySession
            ? transfersPassed
            : notEnoughTimeScenario || (mockCallsPassed && transfersPassed);

        var blockingIssues = new List<string>();
        if (isSupervisorOnlySession)
        {
            if (!transfersComplete)
            {
                blockingIssues.Add("At least one supervisor transfer attempt must be completed.");
            }
        }
        else
        {
            if (!callsComplete)
            {
                blockingIssues.Add("All required mock calls must be scored before review.");
            }

            if (!hasPassedNewMemberCall || !hasPassedExistingMemberCall)
            {
                blockingIssues.Add("Mock calls must include one passed New Member call and one passed Existing Member call.");
            }

            if (!transfersComplete && !notEnoughTimeScenario)
            {
                blockingIssues.Add("At least one supervisor transfer attempt must be completed.");
            }
        }

        var reasonForFail = ResolveReasonForFail(isPassing, notEnoughTimeScenario, blockingIssues);

        var informationalMessages = new List<string>();
        informationalMessages.Add(showCall3
            ? "Call 3 is required until mock-call pass criteria are met (2 passed calls including New + Existing Member)."
            : "Call 3 is skipped because mock-call pass criteria are already met.");

        if (orderedTransfers.FirstOrDefault(x => x.AttemptNumber == 1)?.Outcome == TransferOutcome.Pass)
        {
            informationalMessages.Add("Transfer 2 is optional because Transfer 1 passed.");
        }

        if (notEnoughTimeScenario)
        {
            informationalMessages.Add("Mock calls passed; supervisor transfers not completed due to time. Mark fail reason as N/A.");
        }

        return new EvaluationResult
        {
            ShowCall3 = showCall3,
            IsComplete = isComplete,
            IsPassing = isPassing,
            RequiresSupervisorFollowUp = transferFollowUp,
            ReasonForFail = reasonForFail,
            BlockingIssues = blockingIssues,
            InformationalMessages = informationalMessages
        };
    }

    public bool ShouldShowCall3(IReadOnlyList<CallRecord> calls)
    {
        var firstTwoCalls = calls
            .Where(x => x.CallNumber == 1 || x.CallNumber == 2)
            .OrderBy(x => x.CallNumber)
            .ToList();

        if (firstTwoCalls.Count < 2)
        {
            return true;
        }

        var firstTwoPass = firstTwoCalls.All(x => x.Outcome == CallOutcome.Pass);
        var hasNewMemberCall = firstTwoCalls.Any(x => x.DonorScenario == DonorScenario.NewDonor);
        var hasExistingMemberCall = firstTwoCalls.Any(x => x.DonorScenario == DonorScenario.ExistingMember);

        return !(firstTwoPass && hasNewMemberCall && hasExistingMemberCall);
    }

    private static bool IsSupervisorOnlySession(IReadOnlyList<CallRecord> calls)
    {
        return calls.Count > 0 && calls.All(x => x.Outcome == CallOutcome.NotScored);
    }

    private static string ResolveReasonForFail(bool isPassing, bool notEnoughTimeScenario, IReadOnlyList<string> blockingIssues)
    {
        if (notEnoughTimeScenario)
        {
            return "N/A";
        }

        if (isPassing)
        {
            return "N/A";
        }

        return blockingIssues.FirstOrDefault() ?? "Criteria not met";
    }

    private static IReadOnlyList<TransferRecord> GetTransfersToEvaluate(IReadOnlyList<TransferRecord> transfers)
    {
        var transfer1 = transfers.FirstOrDefault(x => x.AttemptNumber == 1);
        if (transfer1?.Outcome == TransferOutcome.Pass)
        {
            return [transfer1];
        }

        return transfers.ToList();
    }
}
