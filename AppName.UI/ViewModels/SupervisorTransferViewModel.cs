using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Common;
using AppName.Core.Models.Sessions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class SupervisorTransferViewModel : ViewModelBase
{
    private readonly ISessionStateService _sessionStateService;
    private readonly ILookupTableService _lookupTableService;
    private readonly SemaphoreSlim _lookupLoadLock = new(1, 1);

    public SupervisorTransferViewModel(
        ISessionStateService sessionStateService,
        ILookupTableService lookupTableService)
    {
        _sessionStateService = sessionStateService;
        _lookupTableService = lookupTableService;

        Warnings = new ObservableCollection<AppWarning>();
        SupervisorReasonOptions = new ObservableCollection<string>();

        var initialTransfers = _sessionStateService.CurrentSession?.Transfers
            ?? new List<TransferRecord>();

        Transfers = new ObservableCollection<TransferRecordViewModel>(
            initialTransfers
                .OrderBy(x => x.AttemptNumber)
                .Select(x => new TransferRecordViewModel(x, HandleTransferChanged)));

        RefreshWarnings();

        _sessionStateService.SessionChanged += OnSessionChanged;
        _ = InitializeReasonsAsync();
    }

    public ObservableCollection<TransferRecordViewModel> Transfers { get; }

    public ObservableCollection<AppWarning> Warnings { get; }

    public ObservableCollection<string> SupervisorReasonOptions { get; }

    [ObservableProperty]
    private bool hasWarnings;

    [ObservableProperty]
    private bool isLookupLoading;

    private async Task InitializeReasonsAsync(CancellationToken cancellationToken = default)
    {
        await _lookupLoadLock.WaitAsync(cancellationToken);

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
        }
        finally
        {
            IsLookupLoading = false;
            _lookupLoadLock.Release();
        }
    }

    private void HandleTransferChanged(TransferRecordViewModel transfer)
    {
        _sessionStateService.UpdateTransfer(transfer.ToModel());
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshWarnings();
        RefreshTransferVisibility();
    }

    private void RefreshWarnings()
    {
        var session = _sessionStateService.CurrentSession;
        if (session == null)
        {
            return;
        }

        Warnings.Clear();

        var currentWarnings = _sessionStateService.GetCurrentWarnings() ?? Array.Empty<AppWarning>();
        foreach (var warning in currentWarnings)
        {
            Warnings.Add(warning);
        }

        HasWarnings = Warnings.Count > 0;
    }

    private void RefreshTransferVisibility()
    {
        var session = _sessionStateService.CurrentSession;
        if (session?.Transfers == null)
        {
            return;
        }

        foreach (var transferVm in Transfers)
        {
            var model = session.Transfers.FirstOrDefault(x => x.AttemptNumber == transferVm.AttemptNumber);
            if (model is not null)
            {
                transferVm.Outcome = model.Outcome;
                transferVm.Reason = model.Reason ?? string.Empty;
                transferVm.Notes = model.Notes ?? string.Empty;
                transferVm.FollowUpRequired = model.FollowUpRequired;
               transferVm.FollowUpDate = model.FollowUpDate?.LocalDateTime;
            }
        }
    }
}