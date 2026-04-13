using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
using AppName.Core.Models.Sessions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class CallsViewModel : ViewModelBase
{
    private readonly ISessionStateService _sessionStateService;
    private readonly ILookupTableService _lookupTableService;
    private readonly SemaphoreSlim _lookupLoadLock = new(1, 1);

    public CallsViewModel(
        ISessionStateService sessionStateService,
        ILookupTableService lookupTableService)
    {
        _sessionStateService = sessionStateService;
        _lookupTableService = lookupTableService;

        _sessionStateService.SessionChanged += OnSessionChanged;

        var initialCalls = _sessionStateService.CurrentSession?.Calls
            ?? new List<CallRecord>();

        Calls = new ObservableCollection<CallRecordViewModel>(
            initialCalls
                .OrderBy(x => x.CallNumber)
                .Select(x => new CallRecordViewModel(x, HandleCallChanged)));

        Warnings = new ObservableCollection<AppWarning>();
        CoachingReasonOptions = new ObservableCollection<string>();
        FailReasonOptions = new ObservableCollection<string>();

        _ = LoadLookupOptionsAsync();
        RefreshFromSession();
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
        }
        finally
        {
            IsLookupLoading = false;
            _lookupLoadLock.Release();
        }
    }

    private void HandleCallChanged(CallRecordViewModel call)
    {
        _sessionStateService.UpdateCall(call.ToModel());
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshFromSession();
    }

    private void RefreshFromSession()
    {
        var session = _sessionStateService.CurrentSession;
        if (session == null)
        {
            return;
        }

        var result = session.Result;
        ShowCall3 = result?.ShowCall3 ?? true;
        ProgressPercent = session.ProgressPercent;

        var sessionCalls = session.Calls ?? new List<CallRecord>();

        foreach (var callVm in Calls)
        {
            var model = sessionCalls.FirstOrDefault(x => x.CallNumber == callVm.CallNumber);
            if (model is not null)
            {
                callVm.IsVisible = model.IsVisible;
            }
        }

        Warnings.Clear();
        foreach (var warning in _sessionStateService.GetCurrentWarnings() ?? Array.Empty<AppWarning>())
        {
            Warnings.Add(warning);
        }

        HasWarnings = Warnings.Count > 0;
    }
}