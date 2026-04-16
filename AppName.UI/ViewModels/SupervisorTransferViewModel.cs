using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;
using AppName.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class SupervisorTransferViewModel : ViewModelBase
{
    private readonly ISessionStateService _sessionStateService;
    private readonly ILookupTableService _lookupTableService;
    private readonly IAudioFeedbackService _audioFeedbackService;
    private readonly SemaphoreSlim _reasonLoadLock = new(1, 1);
    private bool _isInitializing = true;
    private bool _hadValidationErrors;

    public SupervisorTransferViewModel(
        ISessionStateService sessionStateService,
        ILookupTableService lookupTableService,
        IAudioFeedbackService audioFeedbackService)
    {
        _sessionStateService = sessionStateService;
        _lookupTableService = lookupTableService;
        _audioFeedbackService = audioFeedbackService;

        Transfers = [];
        Warnings = [];
        SupervisorReasonOptions = [];

        SyncTransfers(_sessionStateService.CurrentSession);
        RefreshFromSession();

        _sessionStateService.SessionChanged += OnSessionChanged;
        _isInitializing = false;

        _ = InitializeReasonsAsync();
    }

    public ObservableCollection<TransferRecordViewModel> Transfers { get; }

    public ObservableCollection<AppWarning> Warnings { get; }

    public ObservableCollection<string> SupervisorReasonOptions { get; }

    [ObservableProperty]
    private bool hasWarnings;

    [ObservableProperty]
    private bool isLookupLoading;

    [ObservableProperty]
    private string transferStatusMessage = string.Empty;

    [RelayCommand]
    private void MarkNcNs()
    {
        TransferStatusMessage = "Marked as NC/NS on Supervisor Call 1.";
        _ = _audioFeedbackService.PlayErrorAsync();
    }

    private async Task InitializeReasonsAsync()
    {
        try
        {
            await LoadReasonsAsync();
        }
        catch (Exception)
        {
            // Keep view-model resilient during startup; empty options are acceptable fallback.
        }
    }

    private async Task LoadReasonsAsync(CancellationToken cancellationToken = default)
    {
        await _reasonLoadLock.WaitAsync(cancellationToken);
        try
        {
            IsLookupLoading = true;
            var lookup = await _lookupTableService.GetLookupTablesAsync(cancellationToken);

            SupervisorReasonOptions.Clear();
            foreach (var option in lookup.SupervisorReasons
                         .Where(x => x.IsEnabled)
                         .OrderBy(x => x.SortOrder)
                         .Select(x => x.Name))
            {
                SupervisorReasonOptions.Add(option);
            }
        }
        catch (OperationCanceledException)
        {
            // No-op by design for view shutdown or explicit cancellation.
        }
        finally
        {
            IsLookupLoading = false;
            _reasonLoadLock.Release();
        }
    }

    private void HandleTransferChanged(TransferRecordViewModel transfer)
    {
        if (_isInitializing)
        {
            return;
        }

        _sessionStateService.UpdateTransfer(transfer.ToModel());
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshFromSession();
    }

    private void RefreshFromSession()
    {
        var session = _sessionStateService.CurrentSession ?? new EvaluationSession();
        SyncTransfers(session);

        Warnings.Clear();
        foreach (var warning in _sessionStateService.GetCurrentWarnings()
                     .Where(x => (x.RelatedSection ?? string.Empty).StartsWith("Transfer", StringComparison.OrdinalIgnoreCase)))
        {
            Warnings.Add(warning);
        }

        HasWarnings = Warnings.Count > 0;

        var hasValidationErrors = Warnings.Any(x => x.Severity == Core.Models.Enums.WarningSeverity.Error);
        if (hasValidationErrors && !_hadValidationErrors)
        {
            _ = _audioFeedbackService.PlayErrorAsync();
        }

        _hadValidationErrors = hasValidationErrors;
    }

    private void SyncTransfers(EvaluationSession session)
    {
        var safeTransfers = session.Transfers ?? [];

        foreach (var model in safeTransfers.OrderBy(x => x.AttemptNumber))
        {
            var existing = Transfers.FirstOrDefault(x => x.AttemptNumber == model.AttemptNumber);
            if (existing is not null)
            {
                continue;
            }

            Transfers.Add(new TransferRecordViewModel(model, HandleTransferChanged));
        }

        var toRemove = Transfers
            .Where(vm => safeTransfers.All(model => model.AttemptNumber != vm.AttemptNumber))
            .ToList();

        foreach (var vm in toRemove)
        {
            Transfers.Remove(vm);
        }

        if (session.Result is null)
        {
            session.Result = new EvaluationResult();
        }

        if (session.Review is null)
        {
            session.Review = new ReviewData();
        }

        session.Calls ??= [];
        session.Transfers ??= [];
        session.Warnings ??= [];
    }
}
