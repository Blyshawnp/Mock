using AppName.Core.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly ISessionStateService _sessionStateService;

    public HomeViewModel(ISessionStateService sessionStateService)
    {
        _sessionStateService = sessionStateService;

        RecentSessions = [];

        TransferHistoryCandidates = [];

        FilteredTransferHistoryCandidates = [];
        RefreshFilteredTransferHistoryCandidates();

        RefreshFromSession();
        _sessionStateService.SessionChanged += (_, _) => RefreshFromSession();
    }

    public ObservableCollection<RecentSessionItem> RecentSessions { get; }

    public ObservableCollection<CandidateHistoryItem> TransferHistoryCandidates { get; }

    public ObservableCollection<CandidateHistoryItem> FilteredTransferHistoryCandidates { get; }

    [ObservableProperty]
    private CandidateHistoryItem? selectedHistoryCandidate;

    [ObservableProperty]
    private string transferCandidateSearch = string.Empty;

    [ObservableProperty]
    private bool showTransferSessionPromptDialog;

    [ObservableProperty]
    private bool showTransferCandidatePickerDialog;

    [ObservableProperty]
    private string welcomeName = string.Empty;

    [ObservableProperty]
    private int totalSessions;

    [ObservableProperty]
    private int passRate;

    [ObservableProperty]
    private int ncnsRate;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string globalCandidateName = string.Empty;

    [ObservableProperty]
    private bool isSupervisorTransferOnlySession;

    [RelayCommand]
    private void StartNewSession()
    {
        IsSupervisorTransferOnlySession = false;
        StatusMessage = "Start New Session clicked.";
    }

    [RelayCommand]
    private void SupervisorTransferOnly()
    {
        ShowTransferSessionPromptDialog = true;
    }

    [RelayCommand]
    private void SessionHistory()
    {
        StatusMessage = "Session History clicked.";
    }

    [RelayCommand]
    private void ConfirmTransferSessionPrompt(string answer)
    {
        ShowTransferSessionPromptDialog = false;

        if (string.Equals(answer, "Yes", StringComparison.OrdinalIgnoreCase))
        {
            ShowTransferCandidatePickerDialog = true;
            StatusMessage = "Select candidate from recent history.";
            return;
        }

        IsSupervisorTransferOnlySession = true;
        StatusMessage = "Supervisor Transfer Only session started without history.";
    }

    [RelayCommand]
    private void CancelTransferCandidatePicker()
    {
        ShowTransferCandidatePickerDialog = false;
        SelectedHistoryCandidate = null;
        TransferCandidateSearch = string.Empty;
        RefreshFilteredTransferHistoryCandidates();
        StatusMessage = "Candidate selection canceled.";
    }

    [RelayCommand(CanExecute = nameof(CanConfirmTransferCandidatePicker))]
    private void ConfirmTransferCandidatePicker()
    {
        if (SelectedHistoryCandidate is null)
        {
            return;
        }

        ShowTransferCandidatePickerDialog = false;
        IsSupervisorTransferOnlySession = true;

        _sessionStateService.UpdateSessionInfo(
            SelectedHistoryCandidate.TesterName.Trim(),
            SelectedHistoryCandidate.CandidateName.Trim(),
            _sessionStateService.CurrentSession.FinalAttempt);

        StatusMessage = $"Loaded candidate '{SelectedHistoryCandidate.CandidateName}' from history. Navigating to Supervisor Call 1.";
    }

    partial void OnTransferCandidateSearchChanged(string value)
    {
        RefreshFilteredTransferHistoryCandidates();
    }

    partial void OnSelectedHistoryCandidateChanged(CandidateHistoryItem? value)
    {
        ConfirmTransferCandidatePickerCommand.NotifyCanExecuteChanged();
    }

    private bool CanConfirmTransferCandidatePicker() => SelectedHistoryCandidate is not null;

    private void RefreshFilteredTransferHistoryCandidates()
    {
        FilteredTransferHistoryCandidates.Clear();

        var filtered = string.IsNullOrWhiteSpace(TransferCandidateSearch)
            ? TransferHistoryCandidates
            : TransferHistoryCandidates.Where(x =>
                x.CandidateName.Contains(TransferCandidateSearch, StringComparison.OrdinalIgnoreCase) ||
                x.TesterName.Contains(TransferCandidateSearch, StringComparison.OrdinalIgnoreCase));

        foreach (var item in filtered.Take(10))
        {
            FilteredTransferHistoryCandidates.Add(item);
        }
    }

    private void RefreshFromSession()
    {
        var session = _sessionStateService.CurrentSession;
        WelcomeName = session.EvaluatorName;
        GlobalCandidateName = session.CandidateName;
    }

    public sealed record RecentSessionItem(DateTimeOffset StartedAt, string CandidateName, string Status);

    public sealed record CandidateHistoryItem(string CandidateName, string TesterName);
}
