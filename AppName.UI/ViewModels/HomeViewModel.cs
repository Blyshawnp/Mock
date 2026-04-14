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

        RecentSessions =
        [
            new RecentSessionItem(DateTimeOffset.Now, "Test", "In Progress"),
            new RecentSessionItem(DateTimeOffset.Now.AddDays(-2), "Test Candidate", "In Progress")
        ];

        RefreshFromSession();
        _sessionStateService.SessionChanged += (_, _) => RefreshFromSession();
    }

    public ObservableCollection<RecentSessionItem> RecentSessions { get; }

    [ObservableProperty]
    private string welcomeName = "Shawn";

    [ObservableProperty]
    private int totalSessions = 2;

    [ObservableProperty]
    private int passRate;

    [ObservableProperty]
    private int ncnsRate;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [RelayCommand]
    private void StartNewSession()
    {
        StatusMessage = "Start New Session clicked.";
    }

    [RelayCommand]
    private void SupervisorTransferOnly()
    {
        StatusMessage = "Supervisor Transfer Only clicked.";
    }

    [RelayCommand]
    private void SessionHistory()
    {
        StatusMessage = "Session History clicked.";
    }

    private void RefreshFromSession()
    {
        var session = _sessionStateService.CurrentSession;
        if (!string.IsNullOrWhiteSpace(session?.EvaluatorName))
        {
            WelcomeName = session.EvaluatorName;
        }
    }

    public sealed record RecentSessionItem(DateTimeOffset StartedAt, string CandidateName, string Status);
}
