using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
using AppName.Core.Models.Enums;
using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;

namespace AppName.Core.Services.Validation;

public sealed class ValidationService : IValidationService
{
    private const string OtherSelection = "Other";

    public IReadOnlyList<AppWarning> ValidateCalls(IReadOnlyList<CallRecord> calls)
    {
        var warnings = new List<AppWarning>();

        foreach (var call in calls.Where(x => x.IsVisible))
        {
            if (call.Outcome == CallOutcome.NotScored)
            {
                warnings.Add(new AppWarning
                {
                    Severity = WarningSeverity.Warning,
                    Code = "CALL_NOT_SCORED",
                    RelatedSection = $"Call {call.CallNumber}",
                    Message = $"Call {call.CallNumber} must be scored as Pass or Fail."
                });
            }

            if (HasOtherWithoutText(call.CoachingSelections, call.OtherCoachingText))
            {
                warnings.Add(new AppWarning
                {
                    Severity = WarningSeverity.Error,
                    Code = "COACHING_OTHER_REQUIRED",
                    RelatedSection = $"Call {call.CallNumber}",
                    Message = $"Call {call.CallNumber}: coaching 'Other' requires a text explanation."
                });
            }

            if (HasOtherWithoutText(call.FailSelections, call.OtherFailText))
            {
                warnings.Add(new AppWarning
                {
                    Severity = WarningSeverity.Error,
                    Code = "FAIL_OTHER_REQUIRED",
                    RelatedSection = $"Call {call.CallNumber}",
                    Message = $"Call {call.CallNumber}: fail reason 'Other' requires a text explanation."
                });
            }
        }

        return warnings;
    }

    public IReadOnlyList<AppWarning> ValidateTransfers(IReadOnlyList<TransferRecord> transfers)
    {
        var warnings = new List<AppWarning>();

        foreach (var transfer in transfers)
        {
            if (transfer.Outcome == TransferOutcome.Fail && transfer.FollowUpRequired && transfer.FollowUpDate is null)
            {
                warnings.Add(new AppWarning
                {
                    Severity = WarningSeverity.Error,
                    Code = "TRANSFER_FOLLOW_UP_DATE_REQUIRED",
                    RelatedSection = $"Transfer {transfer.AttemptNumber}",
                    Message = $"Transfer {transfer.AttemptNumber}: follow-up date is required."
                });
            }
        }

        return warnings;
    }

    public IReadOnlyList<AppWarning> ValidateReviewReadiness(EvaluationSession session, ReviewData reviewData)
    {
        var warnings = new List<AppWarning>();

        if (!session.Result.IsComplete)
        {
            warnings.Add(new AppWarning
            {
                Severity = WarningSeverity.Error,
                Code = "REVIEW_NOT_READY",
                RelatedSection = "Review",
                Message = "Calls and transfers must be complete before submit."
            });
        }

        if (string.IsNullOrWhiteSpace(reviewData.CoachingSummary) || string.IsNullOrWhiteSpace(reviewData.FailSummary))
        {
            warnings.Add(new AppWarning
            {
                Severity = WarningSeverity.Warning,
                Code = "SUMMARY_INCOMPLETE",
                RelatedSection = "Review",
                Message = "Generate or edit both summaries before submit."
            });
        }

        return warnings;
    }

    private static bool HasOtherWithoutText(IReadOnlyCollection<string> selections, string? text)
    {
        return selections.Any(x => x.Equals(OtherSelection, StringComparison.OrdinalIgnoreCase))
               && string.IsNullOrWhiteSpace(text);
    }
}
