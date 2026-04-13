using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class SupervisorTransferViewModel : ViewModelBase
{
    private readonly ISessionStateService _sessionStateService;
    private readonly ILookupTableService _lookupTableService;
    private readonly SemaphoreSlim _reasonLoadLock = new(1, 1);

    public SupervisorTransferViewModel(
        ISessionStateService sessionStateService,
        ILookupTableService lookupTableService)
    {
        _sessionStateService = sessionStateService;
        _lookupTableService = lookupTableService;

        _sessionStateService.SessionChanged += OnSessionChanged;

        Transfers = new ObservableCollection<TransferRecordViewModel>(
            _sessionStateService.CurrentSession.Transfers
                .OrderBy(x => x.AttemptNumber)
                .Select(x => new TransferRecordViewModel(x, HandleTransferChanged)));

        Warnings = new ObservableCollection<AppWarning>();
        SupervisorReasonOptions = new ObservableCollection<string>();

        _ = InitializeReasonsAsync();
        RefreshWarnings();
    }

    public ObservableCollection<TransferRecordViewModel> Transfers { get; }

    public ObservableCollection<AppWarning> Warnings { get; }

    public ObservableCollection<string> SupervisorReasonOptions { get; }

    [ObservableProperty]
    private bool hasWarnings;

    [ObservableProperty]
    private bool isLookupLoading;

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
        _sessionStateService.UpdateTransfer(transfer.ToModel());
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshWarnings();
    }

    private void RefreshWarnings()
    {
        Warnings.Clear();
        foreach (var warning in _sessionStateService.GetCurrentWarnings().Where(x => x.RelatedSection.StartsWith("Transfer", StringComparison.OrdinalIgnoreCase)))
        {
            Warnings.Add(warning);
        }

        HasWarnings = Warnings.Count > 0;
    }
}
