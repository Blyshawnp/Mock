using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Help;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class TutorialViewModel : ViewModelBase
{
    private readonly ITutorialContentService _tutorialContentService;
    private readonly ISettingsService _settingsService;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public TutorialViewModel(
        ITutorialContentService tutorialContentService,
        ISettingsService settingsService)
    {
        _tutorialContentService = tutorialContentService;
        _settingsService = settingsService;

        Categories = [];
        Topics = [];
        Steps = [];

        _ = EnsureLoadedAsync();
    }

    public ObservableCollection<string> Categories { get; }

    public ObservableCollection<TutorialTopic> Topics { get; }

    public ObservableCollection<TutorialStep> Steps { get; }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isLoaded;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string? selectedCategory;

    [ObservableProperty]
    private TutorialTopic? selectedTopic;

    [ObservableProperty]
    private int selectedStepIndex;

    [ObservableProperty]
    private bool tutorialMarkedComplete;

    public TutorialStep? CurrentStep => SelectedStepIndex >= 0 && SelectedStepIndex < Steps.Count
        ? Steps[SelectedStepIndex]
        : null;

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoaded)
        {
            return;
        }

        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            if (IsLoaded)
            {
                return;
            }

            IsBusy = true;

            var topics = await _tutorialContentService.GetTopicsAsync(cancellationToken);
            var groupedCategories = topics
                .Select(x => x.Category)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            Categories.Clear();
            foreach (var category in groupedCategories)
            {
                Categories.Add(category);
            }

            SelectedCategory = Categories.FirstOrDefault();

            var settings = await _settingsService.LoadAsync(cancellationToken);
            TutorialMarkedComplete = settings.TutorialCompleted;
            IsLoaded = true;
            StatusMessage = groupedCategories.Count == 0
                ? "No tutorial topics available."
                : "Tutorial loaded.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Tutorial loading canceled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Tutorial loading failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            _loadLock.Release();
            RefreshState();
        }
    }

    partial void OnSelectedCategoryChanged(string? value)
    {
        _ = RefreshTopicsForCategoryAsync();
    }

    partial void OnSelectedTopicChanged(TutorialTopic? value)
    {
        Steps.Clear();
        if (value is null)
        {
            SelectedStepIndex = -1;
            RefreshState();
            return;
        }

        foreach (var step in value.Steps.OrderBy(x => x.StepNumber))
        {
            Steps.Add(step);
        }

        SelectedStepIndex = Steps.Count > 0 ? 0 : -1;
        RefreshState();
    }

    partial void OnSelectedStepIndexChanged(int value) => RefreshState();

    [RelayCommand(CanExecute = nameof(CanMovePrevStep))]
    private void PreviousStep()
    {
        if (SelectedStepIndex > 0)
        {
            SelectedStepIndex--;
        }
    }

    [RelayCommand(CanExecute = nameof(CanMoveNextStep))]
    private void NextStep()
    {
        if (SelectedStepIndex < Steps.Count - 1)
        {
            SelectedStepIndex++;
        }
    }

    [RelayCommand(CanExecute = nameof(CanMarkComplete))]
    private async Task MarkTutorialCompleteAsync()
    {
        IsBusy = true;
        try
        {
            var settings = await _settingsService.LoadAsync();
            settings.TutorialCompleted = true;
            await _settingsService.SaveAsync(settings);
            TutorialMarkedComplete = true;
            StatusMessage = "Tutorial completion saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to save tutorial completion: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RefreshState();
        }
    }

    private bool CanMovePrevStep() => !IsBusy && SelectedStepIndex > 0;

    private bool CanMoveNextStep() => !IsBusy && SelectedStepIndex >= 0 && SelectedStepIndex < Steps.Count - 1;

    private bool CanMarkComplete() => !IsBusy && SelectedTopic is not null;

    private async Task RefreshTopicsForCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedCategory))
        {
            Topics.Clear();
            SelectedTopic = null;
            return;
        }

        try
        {
            IsBusy = true;
            var allTopics = await _tutorialContentService.GetTopicsAsync();

            Topics.Clear();
            foreach (var topic in allTopics
                         .Where(x => string.Equals(x.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase))
                         .OrderBy(x => x.Title))
            {
                Topics.Add(topic);
            }

            SelectedTopic = Topics.FirstOrDefault();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unable to refresh tutorial topics: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            RefreshState();
        }
    }

    private void RefreshState()
    {
        OnPropertyChanged(nameof(CurrentStep));

        PreviousStepCommand.NotifyCanExecuteChanged();
        NextStepCommand.NotifyCanExecuteChanged();
        MarkTutorialCompleteCommand.NotifyCanExecuteChanged();
    }
}
