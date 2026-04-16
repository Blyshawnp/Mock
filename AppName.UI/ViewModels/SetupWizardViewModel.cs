using AppName.Core.Interfaces.Services;
using AppName.UI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class SetupWizardViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ISessionStateService _sessionStateService;
    private readonly IAudioFeedbackService _audioFeedbackService;

    public SetupWizardViewModel(
        ISettingsService settingsService,
        ISessionStateService sessionStateService,
        IAudioFeedbackService audioFeedbackService)
    {
        _settingsService = settingsService;
        _sessionStateService = sessionStateService;
        _audioFeedbackService = audioFeedbackService;
        PronounOptions = ["", "He/Him", "She/Her", "They/Them"];
        ValidationErrors = [];
        _ = InitializeAsync();
    }

    public IReadOnlyList<string> PronounOptions { get; }

    public ObservableCollection<string> ValidationErrors { get; }

    public bool HasValidationErrors => ValidationErrors.Count > 0;

    public bool FinalAttempt
    {
        get => _sessionStateService.CurrentSession.FinalAttempt;
        set
        {
            if (_sessionStateService.CurrentSession.FinalAttempt == value)
            {
                return;
            }

            _sessionStateService.UpdateSessionInfo(TesterName, CandidateName, value);
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    private string testerName = string.Empty;

    [ObservableProperty]
    private string candidateName = string.Empty;

    [ObservableProperty]
    private string selectedPronouns = string.Empty;

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
        var session = _sessionStateService.CurrentSession;
        TesterName = session.EvaluatorName;
        CandidateName = session.CandidateName;

        try
        {
            var settings = await _settingsService.LoadAsync();
            if (string.IsNullOrWhiteSpace(TesterName))
            {
                TesterName = settings.DisplayName?.Trim() ?? string.Empty;
            }
        }
        catch
        {
            // Keep existing session-derived values when settings fail to load.
        }

        OnPropertyChanged(nameof(FinalAttempt));
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
        _ = _audioFeedbackService.PlayErrorAsync();
    }

    [RelayCommand]
    private void MarkNotReady()
    {
        StatusMessage = "Marked as Not Ready.";
        _ = _audioFeedbackService.PlayErrorAsync();
    }

    [RelayCommand]
    private void MarkNcNs()
    {
        StatusMessage = "Marked as NC / NS.";
        _ = _audioFeedbackService.PlayErrorAsync();
    }

    [RelayCommand(CanExecute = nameof(CanContinueBasics))]
    private async Task ContinueBasics()
    {
        if (!ValidateForm())
        {
            StatusMessage = "Please complete all required fields before continuing.";
            _ = _audioFeedbackService.PlayErrorAsync();
            return;
        }

        _sessionStateService.UpdateSessionInfo(TesterName.Trim(), CandidateName.Trim(), FinalAttempt);

        try
        {
            var settings = await _settingsService.LoadAsync();
            settings.DisplayName = TesterName.Trim();
            await _settingsService.SaveAsync(settings);
        }
        catch
        {
            // Session state is authoritative for current run; settings persistence is best-effort.
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
