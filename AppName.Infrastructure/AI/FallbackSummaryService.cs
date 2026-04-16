using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Enums;
using AppName.Core.Models.Sessions;
using System.Text;

namespace AppName.Infrastructure.AI;

public sealed class FallbackSummaryService : ISummaryService
{
    private readonly ILogService _logService;

    public FallbackSummaryService(ILogService logService)
    {
        _logService = logService;
    }

    public async Task<string> GenerateCoachingSummaryAsync(EvaluationSession session, CancellationToken cancellationToken = default)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Coaching Summary");

        var visibleCalls = session.Calls.Where(x => x.IsVisible).OrderBy(x => x.CallNumber).ToList();
        foreach (var call in visibleCalls)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reasons = call.CoachingSelections.Count == 0 ? "No coaching reasons selected" : string.Join(", ", call.CoachingSelections);
            builder.AppendLine($"- Call {call.CallNumber}: {reasons}");
        }

        var summary = builder.ToString().Trim();
        await TryLogAsync($"Generated fallback coaching summary for session {session.SessionId}.", cancellationToken);
        return summary;
    }

    public async Task<string> GenerateFailSummaryAsync(EvaluationSession session, CancellationToken cancellationToken = default)
    {
        var failedCalls = session.Calls.Where(x => x.IsVisible && x.Outcome == CallOutcome.Fail).OrderBy(x => x.CallNumber).ToList();
        var failedTransfers = session.Transfers.Where(x => x.Outcome == TransferOutcome.Fail).OrderBy(x => x.AttemptNumber).ToList();

        if (failedCalls.Count == 0 && failedTransfers.Count == 0)
        {
            await TryLogAsync($"Generated fallback fail summary for session {session.SessionId}: N/A.", cancellationToken);
            return "Fail Summary
- N/A";
        }

        var builder = new StringBuilder();
        builder.AppendLine("Fail Summary");

        foreach (var call in failedCalls)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reasons = call.FailSelections.Count == 0 ? "No fail reasons selected" : string.Join(", ", call.FailSelections);
            builder.AppendLine($"- Call {call.CallNumber}: {reasons}");
        }

        foreach (var transfer in failedTransfers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reason = string.IsNullOrWhiteSpace(transfer.Reason) ? "No reason recorded" : transfer.Reason;
            builder.AppendLine($"- Transfer {transfer.AttemptNumber}: {reason}");
        }

        var summary = builder.ToString().Trim();
        await TryLogAsync($"Generated fallback fail summary for session {session.SessionId}.", cancellationToken);
        return summary;
    }

    private async Task TryLogAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            await _logService.LogInfoAsync(message, cancellationToken);
        }
        catch
        {
            // Summary generation should not fail because logging failed.
        }
    }
}
