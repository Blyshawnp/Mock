using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;
using AppName.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text;

namespace AppName.UI.ViewModels;

public partial class ReviewViewModel : ViewModelBase
{
    private readonly ISessionStateService _sessionStateService;
    private readonly ISummaryService _summaryService;
    private readonly ILogService _logService;
    private readonly IAudioFeedbackService _audioFeedbackService;
    private bool _isInitializing = true;
    private bool _hadValidationErrors;

    public ReviewViewModel(
        ISessionStateService sessionStateService,
        ISummaryService summaryService,
        ILogService logService,
        IAudioFeedbackService audioFeedbackService)
    {
        _sessionStateService = sessionStateService;
        _summaryService = summaryService;
        _logService = logService;
        _audioFeedbackService = audioFeedbackService;

        Warnings = [];

        LoadFromSession();

        _sessionStateService.SessionChanged += OnSessionChanged;
        _isInitializing = false;
    }

    public ObservableCollection<AppWarning> Warnings { get; }

    [ObservableProperty]
    private string sessionSummary = string.Empty;

    [ObservableProperty]
    private string coachingSummary = string.Empty;

    [ObservableProperty]
    private string failSummary = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isReadyForSubmit;

    [ObservableProperty]
    private bool submitConfirmed;

    [ObservableProperty]
    private bool hasWarnings;

    [ObservableProperty]
    private bool coachingSummaryNeedsRegeneration;

    [ObservableProperty]
    private bool failSummaryNeedsRegeneration;

    [ObservableProperty]
    private string summaryStatusMessage = string.Empty;

    [ObservableProperty]
    private bool showSubmitReviewDialog;

    [RelayCommand(CanExecute = nameof(CanRunSummaryActions))]
    private async Task RegenerateCoachingSummaryAsync()
    {
        IsBusy = true;
        SummaryStatusMessage = "Generating coaching summary...";
        try
        {
            CoachingSummary = await _summaryService.GenerateCoachingSummaryAsync(_sessionStateService.CurrentSession);
            CoachingSummaryNeedsRegeneration = false;
            SaveReviewState(coachingGenerated: true, failGenerated: null);
            SummaryStatusMessage = "Coaching summary generated.";
        }
        catch (Exception ex)
        {
            SummaryStatusMessage = "Unable to generate coaching summary.";
            await _logService.LogErrorAsync("Coaching summary generation failed.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRunSummaryActions))]
    private async Task RegenerateFailSummaryAsync()
    {
        IsBusy = true;
        SummaryStatusMessage = "Generating fail summary...";
        try
        {
            FailSummary = await _summaryService.GenerateFailSummaryAsync(_sessionStateService.CurrentSession);
            FailSummaryNeedsRegeneration = false;
            SaveReviewState(coachingGenerated: null, failGenerated: true);
            SummaryStatusMessage = "Fail summary generated.";
        }
        catch (Exception ex)
        {
            SummaryStatusMessage = "Unable to generate fail summary.";
            await _logService.LogErrorAsync("Fail summary generation failed.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRunSummaryActions))]
    private void RetrySummaries()
    {
        var hasCoaching = !string.IsNullOrWhiteSpace(CoachingSummary);
        var hasFail = !string.IsNullOrWhiteSpace(FailSummary);

        CoachingSummaryNeedsRegeneration = !hasCoaching;
        FailSummaryNeedsRegeneration = !hasFail;

        SaveReviewState(
            coachingGenerated: hasCoaching ? null : false,
            failGenerated: hasFail ? null : false);

        SummaryStatusMessage = "Retry flags updated.";
    }

    [RelayCommand(CanExecute = nameof(CanOpenSubmitReviewPopup))]
    private void OpenSubmitReviewPopup()
    {
        ShowSubmitReviewDialog = true;
        _ = _audioFeedbackService.PlaySuccessAsync();
    }

    [RelayCommand(CanExecute = nameof(CanSubmitReview))]
    private void SubmitReview()
    {
        ShowSubmitReviewDialog = false;
        SubmitConfirmed = true;
        SaveReviewState(coachingGenerated: null, failGenerated: null);
        SummaryStatusMessage = "Review submitted.";
    }

    [RelayCommand]
    private void CancelSubmitReviewPopup()
    {
        ShowSubmitReviewDialog = false;
    }

    partial void OnCoachingSummaryChanged(string value)
    {
        SaveReviewState(coachingGenerated: null, failGenerated: null);
    }

    partial void OnFailSummaryChanged(string value)
    {
        SaveReviewState(coachingGenerated: null, failGenerated: null);
    }

    partial void OnIsBusyChanged(bool value)
    {
        RegenerateCoachingSummaryCommand.NotifyCanExecuteChanged();
        RegenerateFailSummaryCommand.NotifyCanExecuteChanged();
        RetrySummariesCommand.NotifyCanExecuteChanged();
        OpenSubmitReviewPopupCommand.NotifyCanExecuteChanged();
        SubmitReviewCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsReadyForSubmitChanged(bool value)
    {
        OpenSubmitReviewPopupCommand.NotifyCanExecuteChanged();
        SubmitReviewCommand.NotifyCanExecuteChanged();
    }

    private bool CanRunSummaryActions() => !IsBusy;

    private bool CanOpenSubmitReviewPopup() => !IsBusy && IsReadyForSubmit;

    private bool CanSubmitReview() => !IsBusy && IsReadyForSubmit;

    private void LoadFromSession()
    {
        var session = _sessionStateService.CurrentSession ?? new EvaluationSession();
        var review = session.Review ?? new ReviewData();
        CoachingSummary = review.CoachingSummary ?? string.Empty;
        FailSummary = review.FailSummary ?? string.Empty;
        SubmitConfirmed = review.SubmitConfirmed;

        RefreshWarningsAndReadiness();
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshWarningsAndReadiness();
    }

    private void RefreshWarningsAndReadiness()
    {
        var session = _sessionStateService.CurrentSession ?? new EvaluationSession();

        Warnings.Clear();
        foreach (var warning in _sessionStateService.GetCurrentWarnings() ?? Array.Empty<AppWarning>())
        {
            Warnings.Add(warning);
        }

        SessionSummary = BuildSessionSummary(session);
        HasWarnings = Warnings.Count > 0;
        var hasValidationErrors = Warnings.Any(x => x.Severity == Core.Models.Enums.WarningSeverity.Error);
        if (!_isInitializing && hasValidationErrors && !_hadValidationErrors)
        {
            _ = _audioFeedbackService.PlayErrorAsync();
        }

        _hadValidationErrors = hasValidationErrors;

        IsReadyForSubmit = !hasValidationErrors
                           && !string.IsNullOrWhiteSpace(CoachingSummary)
                           && !string.IsNullOrWhiteSpace(FailSummary);
    }

    private void SaveReviewState(bool? coachingGenerated, bool? failGenerated)
    {
        if (_isInitializing)
        {
            return;
        }

        var existing = _sessionStateService.CurrentSession?.Review ?? new ReviewData();

        var reviewData = new ReviewData
        {
            CoachingSummary = CoachingSummary,
            FailSummary = FailSummary,
            CoachingSummaryGenerated = coachingGenerated ?? existing.CoachingSummaryGenerated,
            FailSummaryGenerated = failGenerated ?? existing.FailSummaryGenerated,
            ReadyForSubmit = IsReadyForSubmit,
            SubmitConfirmed = SubmitConfirmed
        };

        _sessionStateService.UpdateReview(reviewData);
    }

    private static string BuildSessionSummary(EvaluationSession session)
    {
        var result = session.Result ?? new EvaluationResult();

        var builder = new StringBuilder();
        builder.AppendLine($"Evaluator: {session.EvaluatorName}");
        builder.AppendLine($"Progress: {session.ProgressPercent:F0}%");
        builder.AppendLine($"Passing: {(result.IsPassing ? "Yes" : "No")}");
        builder.AppendLine($"Supervisor Follow-Up Required: {(result.RequiresSupervisorFollowUp ? "Yes" : "No")}");
        builder.AppendLine($"Reason For Fail: {result.ReasonForFail}");

        builder.AppendLine("Calls:");
        foreach (var call in (session.Calls ?? []).Where(x => x.IsVisible))
        {
            builder.AppendLine($"  - Call {call.CallNumber}: {call.Outcome}");
        }

        builder.AppendLine("Transfers:");
        foreach (var transfer in session.Transfers ?? [])
        {
            builder.AppendLine($"  - Attempt {transfer.AttemptNumber}: {transfer.Outcome} (Follow-Up: {(transfer.FollowUpRequired ? "Yes" : "No")})");
        }

        return builder.ToString().TrimEnd();
    }
}
