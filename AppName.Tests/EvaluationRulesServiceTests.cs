using AppName.Core.Models.Enums;
using AppName.Core.Models.Sessions;
using AppName.Core.Services.Rules;

namespace AppName.Tests;

public sealed class EvaluationRulesServiceTests
{
    private readonly EvaluationRulesService _service = new();

    [Fact]
    public void Evaluate_FullSessionPasses_WhenTwoCallsPass_WithNewAndExisting_AndOneTransferPasses()
    {
        var session = CreateSession();
        session.Calls[0].Outcome = CallOutcome.Pass; // New donor
        session.Calls[1].Outcome = CallOutcome.Pass; // Existing member
        session.Transfers[0].Outcome = TransferOutcome.Pass;

        var result = _service.Evaluate(session);

        Assert.False(result.ShowCall3);
        Assert.True(result.IsPassing);
        Assert.True(result.IsComplete);
    }

    [Fact]
    public void Evaluate_Call3RemainsRequired_WhenTwoPassesLackExistingMemberPass()
    {
        var session = CreateSession();
        session.Calls[0].Outcome = CallOutcome.Pass; // New donor
        session.Calls[1].Outcome = CallOutcome.Fail; // Existing member failed
        session.Calls[2].Outcome = CallOutcome.Pass; // Increase sustaining pass

        var result = _service.Evaluate(session);

        Assert.True(result.ShowCall3);
        Assert.False(result.IsPassing);
        Assert.Contains(result.BlockingIssues, issue => issue.Contains("New Member", StringComparison.OrdinalIgnoreCase));
    }


    [Fact]
    public void Evaluate_UnknownScenarioPass_DoesNotSatisfyNewExistingRequirement()
    {
        var session = CreateSession();
        session.Calls[0].DonorScenario = DonorScenario.Unknown;
        session.Calls[0].Outcome = CallOutcome.Pass;
        session.Calls[1].Outcome = CallOutcome.Pass;
        session.Calls[2].Outcome = CallOutcome.NotScored;

        var result = _service.Evaluate(session);

        Assert.True(result.ShowCall3);
        Assert.False(result.IsPassing);
    }

    [Fact]
    public void Evaluate_DoesNotRequireTransfer2_WhenTransfer1Passes()
    {
        var session = CreateSession();
        session.Calls[0].Outcome = CallOutcome.Pass;
        session.Calls[1].Outcome = CallOutcome.Pass;
        session.Transfers[0].Outcome = TransferOutcome.Pass;
        session.Transfers[1].Outcome = TransferOutcome.NotAttempted;

        var result = _service.Evaluate(session);

        Assert.True(result.IsPassing);
        Assert.True(result.IsComplete);
        Assert.Contains(result.InformationalMessages, m => m.Contains("Transfer 2 is optional", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Evaluate_SupervisorOnlySessionPasses_WhenOneTransferPasses()
    {
        var session = CreateSession();
        session.Calls.ForEach(c => c.Outcome = CallOutcome.NotScored);
        session.Transfers[0].Outcome = TransferOutcome.Pass;
        session.Transfers[1].Outcome = TransferOutcome.NotAttempted;

        var result = _service.Evaluate(session);

        Assert.True(result.IsPassing);
        Assert.True(result.IsComplete);
    }

    [Fact]
    public void Evaluate_NotEnoughTime_IsNotFail_AndMarksNAReasonMessage()
    {
        var session = CreateSession();
        session.Calls[0].Outcome = CallOutcome.Pass;
        session.Calls[1].Outcome = CallOutcome.Pass;
        session.Transfers[0].Outcome = TransferOutcome.NotAttempted;
        session.Transfers[1].Outcome = TransferOutcome.NotAttempted;

        var result = _service.Evaluate(session);

        Assert.True(result.IsPassing);
        Assert.True(result.IsComplete);
        Assert.Contains(result.InformationalMessages, m => m.Contains("N/A", StringComparison.OrdinalIgnoreCase));
    }

    private static EvaluationSession CreateSession()
    {
        return new EvaluationSession
        {
            Calls =
            [
                new CallRecord { CallNumber = 1, DonorScenario = DonorScenario.NewDonor, Outcome = CallOutcome.NotScored, IsVisible = true },
                new CallRecord { CallNumber = 2, DonorScenario = DonorScenario.ExistingMember, Outcome = CallOutcome.NotScored, IsVisible = true },
                new CallRecord { CallNumber = 3, DonorScenario = DonorScenario.IncreaseSustaining, Outcome = CallOutcome.NotScored, IsVisible = true }
            ],
            Transfers =
            [
                new TransferRecord { AttemptNumber = 1, Outcome = TransferOutcome.NotAttempted },
                new TransferRecord { AttemptNumber = 2, Outcome = TransferOutcome.NotAttempted }
            ]
        };
    }
}
