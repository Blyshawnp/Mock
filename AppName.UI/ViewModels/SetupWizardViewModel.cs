using AppName.Core.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class SetupWizardViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public SetupWizardViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        PronounOptions = ["", "He/Him", "She/Her", "They/Them"];
        ValidationErrors = [];
        _ = InitializeAsync();
    }

    public IReadOnlyList<string> PronounOptions { get; }

    public ObservableCollection<string> ValidationErrors { get; }

    public bool HasValidationErrors => ValidationErrors.Count > 0;

    [ObservableProperty]
    private string testerName = string.Empty;

    [ObservableProperty]
    private string candidateName = string.Empty;

    [ObservableProperty]
    private string selectedPronouns = string.Empty;

    [ObservableProperty]
    private bool finalAttempt;

    [ObservableProperty]
    private bool? isHeadsetUsb;

    [ObservableProperty]
    private bool? hasNoiseCancellingMic;

    [ObservableProperty]
    private string headsetModel = string.Empty;

    [ObservableProperty]
    private bool? hasVpn;

    [ObservableProperty]
    private bool? vpnCanTurnOff;

    [ObservableProperty]
    private bool? defaultBrowserSet;

    [ObservableProperty]
    private bool? extensionsOff;

    [ObservableProperty]
    private bool? popupsAllowed;

    [ObservableProperty]
    private bool showTechIssueDialog;

    [ObservableProperty]
    private bool showTransferConfirmDialog;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isFormValid;

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

        RefreshValidationState();
    }

    partial void OnTesterNameChanged(string value) => RefreshValidationState();
    partial void OnCandidateNameChanged(string value) => RefreshValidationState();
    partial void OnHeadsetModelChanged(string value) => RefreshValidationState();
    partial void OnIsHeadsetUsbChanged(bool? value) => RefreshValidationState();
    partial void OnHasNoiseCancellingMicChanged(bool? value) => RefreshValidationState();
    partial void OnHasVpnChanged(bool? value) => RefreshValidationState();
    partial void OnVpnCanTurnOffChanged(bool? value) => RefreshValidationState();
    partial void OnDefaultBrowserSetChanged(bool? value) => RefreshValidationState();
    partial void OnExtensionsOffChanged(bool? value) => RefreshValidationState();
    partial void OnPopupsAllowedChanged(bool? value) => RefreshValidationState();

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

    [RelayCommand(CanExecute = nameof(CanContinueBasics))]
    private void ContinueBasics()
    {
        if (!ValidateForm())
        {
            StatusMessage = "Please complete all required fields before continuing.";
            return;
        }

        StatusMessage = "Basics complete. Continue clicked.";
    }

    private bool CanContinueBasics() => IsFormValid;

    private void RefreshValidationState()
    {
        IsFormValid = ValidateForm();
        ContinueBasicsCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    private bool ValidateForm()
    {
        ValidationErrors.Clear();

        if (string.IsNullOrWhiteSpace(TesterName))
        {
            ValidationErrors.Add("Tester Name is required.");
        }

        if (string.IsNullOrWhiteSpace(CandidateName))
        {
            ValidationErrors.Add("Candidate Name is required.");
        }

        if (string.IsNullOrWhiteSpace(HeadsetModel))
        {
            ValidationErrors.Add("Headset Brand / Model is required.");
        }

        if (IsHeadsetUsb is null)
        {
            ValidationErrors.Add("Headset USB selection is required.");
        }

        if (HasNoiseCancellingMic is null)
        {
            ValidationErrors.Add("Noise Cancelling Mic selection is required.");
        }

        if (HasVpn is null)
        {
            ValidationErrors.Add("Has VPN selection is required.");
        }

        if (HasVpn is true && VpnCanTurnOff is null)
        {
            ValidationErrors.Add("Can turn off VPN selection is required when VPN is enabled.");
        }

        if (DefaultBrowserSet is null)
        {
            ValidationErrors.Add("Default browser selection is required.");
        }

        if (ExtensionsOff is null)
        {
            ValidationErrors.Add("Extensions Off selection is required.");
        }

        if (PopupsAllowed is null)
        {
            ValidationErrors.Add("Pop-ups Allowed selection is required.");
        }

        return ValidationErrors.Count == 0;
    }
}
