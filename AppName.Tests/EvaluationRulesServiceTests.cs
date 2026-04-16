using AppName.Core.Models.Enums;
using AppName.Core.Models.Sessions;
using AppName.Core.Services.Rules;

namespace AppName.Tests;

public sealed class EvaluationRulesServiceTests
{
    private readonly EvaluationRulesService _service = new();

    [Fact]
    public void Evaluate_FullSessionPasses_WhenTwoCallsPassAndOneTransferPasses()
    {
        var session = CreateSession();
        session.Calls[0].Outcome = CallOutcome.Pass;
        session.Calls[1].Outcome = CallOutcome.Pass;
        session.Transfers[0].Outcome = TransferOutcome.Pass;

        var result = _service.Evaluate(session);

        Assert.False(result.ShowCall3);
        Assert.True(result.IsPassing);
        Assert.True(result.IsComplete);
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
                new CallRecord { CallNumber = 1, Outcome = CallOutcome.NotScored, IsVisible = true },
                new CallRecord { CallNumber = 2, Outcome = CallOutcome.NotScored, IsVisible = true },
                new CallRecord { CallNumber = 3, Outcome = CallOutcome.NotScored, IsVisible = true }
            ],
            Transfers =
            [
                new TransferRecord { AttemptNumber = 1, Outcome = TransferOutcome.NotAttempted },
                new TransferRecord { AttemptNumber = 2, Outcome = TransferOutcome.NotAttempted }
            ]
        };
    }
}
