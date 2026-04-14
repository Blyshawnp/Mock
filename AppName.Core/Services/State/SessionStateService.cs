using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
using AppName.Core.Models.Enums;
using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;

namespace AppName.Core.Services.State;

public sealed class SessionStateService : ISessionStateService
{
    private readonly IEvaluationRulesService _evaluationRulesService;
    private readonly IValidationService _validationService;
    private readonly ISessionRepository? _sessionRepository;
    private readonly ILogService? _logService;
    private readonly SemaphoreSlim _persistenceLock = new(1, 1);
    private long _stateVersion;

    public SessionStateService(
        IEvaluationRulesService evaluationRulesService,
        IValidationService validationService,
        ISessionRepository? sessionRepository = null,
        ILogService? logService = null)
    {
        _evaluationRulesService = evaluationRulesService;
        _validationService = validationService;
        _sessionRepository = sessionRepository;
        _logService = logService;

        CurrentSession = new EvaluationSession();
        ApplyRulesAndValidation();

        _ = LoadInitialSessionAsync();
    }

    public EvaluationSession CurrentSession { get; private set; }

    public event EventHandler? SessionChanged;

    public void InitializeNewSession(string evaluatorName)
    {
        MarkStateMutated();
        CurrentSession = new EvaluationSession
        {
            EvaluatorName = evaluatorName,
            Status = SessionStatus.InProgress,
            CurrentScreenKey = "Calls"
        };

        ApplyRulesAndValidation();
        _ = PersistSessionAsync();
    }

    public void UpdateCall(CallRecord updatedCall)
    {
        var existing = CurrentSession.Calls.FirstOrDefault(x => x.CallNumber == updatedCall.CallNumber);
        if (existing is null)
        {
            return;
        }

        MarkStateMutated();
        existing.Outcome = updatedCall.Outcome;
        existing.CoachingSelections = [.. updatedCall.CoachingSelections];
        existing.FailSelections = [.. updatedCall.FailSelections];
        existing.OtherCoachingText = updatedCall.OtherCoachingText;
        existing.OtherFailText = updatedCall.OtherFailText;
        existing.Notes = updatedCall.Notes;
        existing.IsComplete = updatedCall.IsComplete;

        ApplyRulesAndValidation();
        _ = PersistSessionAsync();
    }

    public void UpdateTransfer(TransferRecord updatedTransfer)
    {
        var existing = CurrentSession.Transfers.FirstOrDefault(x => x.AttemptNumber == updatedTransfer.AttemptNumber);
        if (existing is null)
        {
            return;
        }

        MarkStateMutated();
        existing.Outcome = updatedTransfer.Outcome;
        existing.Reason = updatedTransfer.Reason;
        existing.Notes = updatedTransfer.Notes;
        existing.FollowUpRequired = updatedTransfer.FollowUpRequired;
        existing.FollowUpDate = updatedTransfer.FollowUpDate;

        ApplyRulesAndValidation();
        _ = PersistSessionAsync();
    }

    public void UpdateReview(ReviewData reviewData)
    {
        MarkStateMutated();
        CurrentSession.Review = reviewData;
        ApplyRulesAndValidation();
        _ = PersistSessionAsync();
    }

    public IReadOnlyList<AppWarning> GetCurrentWarnings()
    {
        return CurrentSession.Warnings;
    }

    public double GetProgressPercent()
    {
        return CurrentSession.ProgressPercent;
    }

    private async Task LoadInitialSessionAsync(CancellationToken cancellationToken = default)
    {
        if (_sessionRepository is null)
        {
            return;
        }

        var versionBeforeLoad = Interlocked.Read(ref _stateVersion);

        try
        {
            var existing = await _sessionRepository.LoadLatestAsync(cancellationToken);
            if (existing is null)
            {
                return;
            }

            if (Interlocked.Read(ref _stateVersion) != versionBeforeLoad)
            {
                await TryLogInfoAsync("Skipped initial session restore because in-memory state changed before restore completed.", cancellationToken);
                return;
            }

            CurrentSession = existing;
            ApplyRulesAndValidation();
            await TryLogInfoAsync($"Loaded existing session: {CurrentSession.SessionId}", cancellationToken);
        }
        catch (Exception ex)
        {
            await TryLogErrorAsync("Failed to load initial session.", ex, cancellationToken);
        }
    }

    private async Task PersistSessionAsync(CancellationToken cancellationToken = default)
    {
        if (_sessionRepository is null)
        {
            return;
        }

        await _persistenceLock.WaitAsync(cancellationToken);
        try
        {
            await _sessionRepository.SaveAsync(CurrentSession, cancellationToken);
        }
        catch (Exception ex)
        {
            await TryLogErrorAsync("Failed to persist session state.", ex, cancellationToken);
        }
        finally
        {
            _persistenceLock.Release();
        }
    }

    private void ApplyRulesAndValidation()
    {
        CurrentSession ??= new EvaluationSession();
        CurrentSession.Calls ??= [];
        CurrentSession.Transfers ??= [];
        CurrentSession.Review ??= new ReviewData();
        CurrentSession.Warnings ??= [];

        var evaluatedResult = _evaluationRulesService.Evaluate(CurrentSession);
        CurrentSession.Result = evaluatedResult ?? new EvaluationResult();

        var call3 = CurrentSession.Calls.FirstOrDefault(x => x.CallNumber == 3);
        if (call3 is not null)
        {
            call3.IsVisible = CurrentSession.Result.ShowCall3;
        }

        var callWarnings = _validationService.ValidateCalls(CurrentSession.Calls);
        var transferWarnings = _validationService.ValidateTransfers(CurrentSession.Transfers);
        var reviewWarnings = _validationService.ValidateReviewReadiness(CurrentSession, CurrentSession.Review);

        CurrentSession.Warnings = [.. callWarnings, .. transferWarnings, .. reviewWarnings];

        var visibleCalls = CurrentSession.Calls.Where(x => x.IsVisible).ToList();
        var completedCalls = visibleCalls.Count(x => x.Outcome != CallOutcome.NotScored);

        var completedTransfers = CurrentSession.Transfers.Count(x => x.Outcome != TransferOutcome.NotAttempted);
        var totalUnits = visibleCalls.Count + CurrentSession.Transfers.Count;
        var completedUnits = completedCalls + completedTransfers;

        CurrentSession.ProgressPercent = totalUnits == 0
            ? 0
            : Math.Round(completedUnits * 100d / totalUnits, 2);

        CurrentSession.UpdatedAt = DateTimeOffset.UtcNow;

        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void MarkStateMutated()
    {
        Interlocked.Increment(ref _stateVersion);
    }

    private async Task TryLogInfoAsync(string message, CancellationToken cancellationToken)
    {
        if (_logService is null)
        {
            return;
        }

        try
        {
            await _logService.LogInfoAsync(message, cancellationToken);
        }
        catch
        {
            // Intentionally no-op. Logging must never break session flow.
        }
    }

    private async Task TryLogErrorAsync(string message, Exception ex, CancellationToken cancellationToken)
    {
        if (_logService is null)
        {
            return;
        }

        try
        {
            await _logService.LogErrorAsync(message, ex, cancellationToken);
        }
        catch
        {
            // Intentionally no-op. Logging must never break session flow.
        }
    }
}
