using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
using AppName.Core.Models.Review;
using AppName.Core.Models.Sessions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class CallsViewModel : ViewModelBase
{
    private readonly ISessionStateService _sessionStateService;
    private readonly ILookupTableService _lookupTableService;
    private readonly SemaphoreSlim _lookupLoadLock = new(1, 1);
    private bool _isInitializing = true;

    public CallsViewModel(
        ISessionStateService sessionStateService,
        ILookupTableService lookupTableService)
    {
        _sessionStateService = sessionStateService;
        _lookupTableService = lookupTableService;

        Calls = [];
        Warnings = [];
        CoachingReasonOptions = [];
        FailReasonOptions = [];

        SyncCalls(_sessionStateService.CurrentSession);
        RefreshFromSession();

        _sessionStateService.SessionChanged += OnSessionChanged;
        _isInitializing = false;

        _ = LoadLookupOptionsAsync();
    }

    public ObservableCollection<CallRecordViewModel> Calls { get; }

    public ObservableCollection<AppWarning> Warnings { get; }

    public ObservableCollection<string> CoachingReasonOptions { get; }

    public ObservableCollection<string> FailReasonOptions { get; }

    [ObservableProperty]
    private bool showCall3;

    [ObservableProperty]
    private double progressPercent;

    [ObservableProperty]
    private bool hasWarnings;

    [ObservableProperty]
    private bool isLookupLoading;

    [ObservableProperty]
    private bool showNoCoachingDialog;

    [ObservableProperty]
    private bool showCallTypeDialog;

    [ObservableProperty]
    private bool showConfirmTransferTimeDialog;

    [ObservableProperty]
    private string callStatusMessage = string.Empty;

    [RelayCommand]
    private void ShowNoCoachingWarning() => ShowNoCoachingDialog = true;

    [RelayCommand]
    private void CloseNoCoachingWarning() => ShowNoCoachingDialog = false;

    [RelayCommand]
    private void ShowCallTypeWarning() => ShowCallTypeDialog = true;

    [RelayCommand]
    private void CloseCallTypeWarning() => ShowCallTypeDialog = false;

    [RelayCommand]
    private void OpenConfirmTransferTime() => ShowConfirmTransferTimeDialog = true;

    [RelayCommand]
    private void ConfirmTransferTime(string answer)
    {
        ShowConfirmTransferTimeDialog = false;
        CallStatusMessage = answer == "Yes"
            ? "Confirmed enough time for Supervisor Transfer."
            : "Transfer time declined.";
    }

    [RelayCommand]
    private void MarkStoppedResponding() => CallStatusMessage = "Marked as Stopped Responding.";

    [RelayCommand]
    private void ContinueCall() => CallStatusMessage = "Continue clicked.";

    private async Task LoadLookupOptionsAsync(CancellationToken cancellationToken = default)
    {
        await _lookupLoadLock.WaitAsync(cancellationToken);

        try
        {
            IsLookupLoading = true;
            var lookup = await _lookupTableService.GetLookupTablesAsync(cancellationToken);

            CoachingReasonOptions.Clear();
            foreach (var option in lookup.CoachingCategories
                         .Where(x => x.IsEnabled)
                         .OrderBy(x => x.SortOrder)
                         .Select(x => x.Name))
            {
                CoachingReasonOptions.Add(option);
            }

            FailReasonOptions.Clear();
            foreach (var option in lookup.FailReasons
                         .Where(x => x.IsEnabled)
                         .OrderBy(x => x.SortOrder)
                         .Select(x => x.Name))
            {
                FailReasonOptions.Add(option);
            }
        }
        catch (OperationCanceledException)
        {
            // No-op by design for view shutdown or explicit cancellation.
        }
        finally
        {
            IsLookupLoading = false;
            _lookupLoadLock.Release();
        }
    }

    private void HandleCallChanged(CallRecordViewModel call)
    {
        if (_isInitializing)
        {
            return;
        }

        _sessionStateService.UpdateCall(call.ToModel());
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshFromSession();
    }

    private void RefreshFromSession()
    {
        var session = _sessionStateService.CurrentSession ?? new EvaluationSession();

        SyncCalls(session);

        ShowCall3 = session.Result?.ShowCall3 ?? true;
        ProgressPercent = session.ProgressPercent;

        foreach (var callVm in Calls)
        {
            var model = session.Calls?.FirstOrDefault(x => x.CallNumber == callVm.CallNumber);
            callVm.IsVisible = model?.IsVisible ?? true;
        }

        Warnings.Clear();
        foreach (var warning in _sessionStateService.GetCurrentWarnings() ?? Array.Empty<AppWarning>())
        {
            Warnings.Add(warning);
        }

        HasWarnings = Warnings.Count > 0;
    }

    private void SyncCalls(EvaluationSession session)
    {
        var safeCalls = session.Calls ?? [];

        foreach (var model in safeCalls.OrderBy(x => x.CallNumber))
        {
            var existing = Calls.FirstOrDefault(x => x.CallNumber == model.CallNumber);
            if (existing is not null)
            {
                continue;
            }

            Calls.Add(new CallRecordViewModel(model, HandleCallChanged));
        }

        var toRemove = Calls
            .Where(vm => safeCalls.All(model => model.CallNumber != vm.CallNumber))
            .ToList();

        foreach (var vm in toRemove)
        {
            Calls.Remove(vm);
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
