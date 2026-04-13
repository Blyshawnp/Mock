using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Config;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppName.UI.ViewModels;

public partial class SetupWizardViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly SemaphoreSlim _initializeLock = new(1, 1);

    public SetupWizardViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        StepTitles = ["Welcome", "Preferences", "Storage", "Review"];
        StatusMessage = "Loading setup...";

        _ = EnsureInitializedAsync();
    }

    public IReadOnlyList<string> StepTitles { get; }

    [ObservableProperty]
    private int currentStepIndex;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isInitialized;

    [ObservableProperty]
    private string theme = "Light";

    [ObservableProperty]
    private bool soundsEnabled = true;

    [ObservableProperty]
    private bool beginnerModeEnabled = true;

    [ObservableProperty]
    private int autosaveMinutes = 5;

    [ObservableProperty]
    private string defaultSaveFolder = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public bool IsFirstStep => CurrentStepIndex == 0;

    public bool IsLastStep => CurrentStepIndex >= StepTitles.Count - 1;

    public string CurrentStepTitle => StepTitles[Math.Clamp(CurrentStepIndex, 0, StepTitles.Count - 1)];

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized)
        {
            return;
        }

        await _initializeLock.WaitAsync(cancellationToken);
        try
        {
            if (IsInitialized)
            {
                return;
            }

            IsBusy = true;
            var settings = await _settingsService.LoadAsync(cancellationToken);

            Theme = string.IsNullOrWhiteSpace(settings.Theme) ? "Light" : settings.Theme;
            SoundsEnabled = settings.SoundsEnabled;
            BeginnerModeEnabled = settings.BeginnerModeEnabled;
            AutosaveMinutes = settings.AutosaveMinutes <= 0 ? 5 : settings.AutosaveMinutes;
            DefaultSaveFolder = settings.DefaultSaveFolder;

            StatusMessage = settings.FirstRunComplete
                ? "Setup already completed. You can review or update these first-run settings."
                : "Complete setup to finish first-run configuration.";

            IsInitialized = true;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Setup initialization canceled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Setup failed to initialize: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            _initializeLock.Release();
            RefreshComputedState();
        }
    }

    [RelayCommand(CanExecute = nameof(CanMoveNext))]
    private void NextStep()
    {
        if (!ValidateCurrentStep(out var error))
        {
            StatusMessage = error;
            return;
        }

        CurrentStepIndex++;
        StatusMessage = string.Empty;
        RefreshComputedState();
    }

    [RelayCommand(CanExecute = nameof(CanMovePrevious))]
    private void PreviousStep()
    {
        if (CurrentStepIndex <= 0)
        {
            return;
        }

        CurrentStepIndex--;
        StatusMessage = string.Empty;
        RefreshComputedState();
    }

    [RelayCommand(CanExecute = nameof(CanFinish))]
    private async Task FinishAsync()
    {
        if (!ValidateAll(out var error))
        {
            StatusMessage = error;
            return;
        }

        IsBusy = true;
        try
        {
            var existing = await _settingsService.LoadAsync();
            var updated = new AppSettings
            {
                Theme = Theme,
                SoundsEnabled = SoundsEnabled,
                BeginnerModeEnabled = BeginnerModeEnabled,
                AutosaveMinutes = AutosaveMinutes,
                DefaultSaveFolder = DefaultSaveFolder,
                FirstRunComplete = true,
                TutorialCompleted = existing.TutorialCompleted
            };

            await _settingsService.SaveAsync(updated);

            StatusMessage = "Setup complete. First-run operational settings were saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to save setup: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RefreshComputedState();
        }
    }

    partial void OnCurrentStepIndexChanged(int value) => RefreshComputedState();

    partial void OnIsBusyChanged(bool value) => RefreshComputedState();

    private bool CanMoveNext() => !IsBusy && !IsLastStep;

    private bool CanMovePrevious() => !IsBusy && !IsFirstStep;

    private bool CanFinish() => !IsBusy && IsLastStep;

    private bool ValidateCurrentStep(out string error)
    {
        error = string.Empty;

        if (CurrentStepIndex == 1)
        {
            if (!string.Equals(Theme, "Light", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(Theme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                error = "Theme must be Light or Dark.";
                return false;
            }

            if (AutosaveMinutes is < 1 or > 60)
            {
                error = "Autosave must be between 1 and 60 minutes.";
                return false;
            }
        }

        if (CurrentStepIndex == 2 && string.IsNullOrWhiteSpace(DefaultSaveFolder))
        {
            error = "Default save folder is required.";
            return false;
        }

        return true;
    }

    private bool ValidateAll(out string error)
    {
        var originalStep = CurrentStepIndex;
        for (var step = 0; step < StepTitles.Count - 1; step++)
        {
            CurrentStepIndex = step;
            if (!ValidateCurrentStep(out error))
            {
                CurrentStepIndex = originalStep;
                RefreshComputedState();
                return false;
            }
        }

        CurrentStepIndex = originalStep;
        RefreshComputedState();
        error = string.Empty;
        return true;
    }

    private void RefreshComputedState()
    {
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(CurrentStepTitle));

        NextStepCommand.NotifyCanExecuteChanged();
        PreviousStepCommand.NotifyCanExecuteChanged();
        FinishCommand.NotifyCanExecuteChanged();
    }
}
