using AppName.Core.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppName.UI.ViewModels;

public partial class SetupWizardViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public SetupWizardViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        PronounOptions = ["", "He/Him", "She/Her", "They/Them"];
        _ = InitializeAsync();
    }

    public IReadOnlyList<string> PronounOptions { get; }

    [ObservableProperty]
    private string testerName = string.Empty;

    [ObservableProperty]
    private string candidateName = string.Empty;

    [ObservableProperty]
    private string selectedPronouns = string.Empty;

    [ObservableProperty]
    private bool finalAttempt;

    [ObservableProperty]
    private bool isHeadsetUsb;

    [ObservableProperty]
    private bool hasNoiseCancellingMic;

    [ObservableProperty]
    private string headsetModel = string.Empty;

    [ObservableProperty]
    private bool hasVpn;

    [ObservableProperty]
    private bool vpnCanTurnOff;

    [ObservableProperty]
    private bool defaultBrowserSet;

    [ObservableProperty]
    private bool extensionsOff;

    [ObservableProperty]
    private bool popupsAllowed;

    [ObservableProperty]
    private bool showTechIssueDialog;

    [ObservableProperty]
    private bool showTransferConfirmDialog;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    private async Task InitializeAsync()
    {
        try
        {
            var settings = await _settingsService.LoadAsync();
            TesterName = string.IsNullOrWhiteSpace(settings.DisplayName) ? "Shawn Bly" : settings.DisplayName;
        }
        catch
        {
            TesterName = "Shawn Bly";
        }
    }

    [RelayCommand]
    private void OpenTechIssueDialog() => ShowTechIssueDialog = true;

    [RelayCommand]
    private void CloseTechIssueDialog() => ShowTechIssueDialog = false;

    [RelayCommand]
    private void ContinueFromTechIssues()
    {
        ShowTechIssueDialog = false;
        ShowTransferConfirmDialog = true;
    }

    [RelayCommand]
    private void ConfirmTransferTime(string result)
    {
        ShowTransferConfirmDialog = false;
        StatusMessage = result == "Yes"
            ? "Confirmed: enough time for Supervisor Transfers."
            : "Not enough time for Supervisor Transfers.";
    }

    [RelayCommand]
    private void MarkStoppedResponding()
    {
        StatusMessage = "Marked as Stopped Responding.";
    }

    [RelayCommand]
    private void MarkNotReady()
    {
        StatusMessage = "Marked as Not Ready.";
    }

    [RelayCommand]
    private void MarkNcNs()
    {
        StatusMessage = "Marked as NC / NS.";
    }

    [RelayCommand]
    private void ContinueBasics()
    {
        StatusMessage = "Basics complete. Continue clicked.";
    }
}
