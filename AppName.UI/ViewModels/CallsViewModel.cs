using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
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

        Calls = new ObservableCollection<CallRecordViewModel>(
            _sessionStateService.CurrentSession.Calls
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
        _sessionStateService.UpdateCall(call.ToModel());
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshFromSession();
    }

    private void RefreshFromSession()
    {
        var session = _sessionStateService.CurrentSession;
        ShowCall3 = session.Result.ShowCall3;
        ProgressPercent = session.ProgressPercent;

        foreach (var callVm in Calls)
        {
            var model = session.Calls.First(x => x.CallNumber == callVm.CallNumber);
            callVm.IsVisible = model.IsVisible;
        }

        Warnings.Clear();
        foreach (var warning in _sessionStateService.GetCurrentWarnings())
        {
            Warnings.Add(warning);
        }

        HasWarnings = Warnings.Count > 0;
    }
}
