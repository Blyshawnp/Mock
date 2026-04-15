using AppName.Core.Interfaces.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class NewbieShiftViewModel : ViewModelBase
{
    private readonly ITutorialContentService _tutorialContentService;
    private readonly IHelpContentService _helpContentService;

    public NewbieShiftViewModel(
        ITutorialContentService tutorialContentService,
        IHelpContentService helpContentService)
    {
        _tutorialContentService = tutorialContentService;
        _helpContentService = helpContentService;

        QuickReferences = [];
        Reminders =
        [
            "Score every required call before switching tabs.",
            "If transfer follow-up is required, always set a follow-up date.",
            "Use 'Other' only when no existing reason applies."
        ];
        CommonMistakes =
        [
            "Submitting review before both summaries are generated.",
            "Leaving donor email blank in profile-driven workflows.",
            "Forgetting to re-check Call 3 visibility rule when outcomes change."
        ];

        _ = LoadQuickReferenceAsync();
    }

    public ObservableCollection<QuickReferenceItem> QuickReferences { get; }

    public ObservableCollection<string> Reminders { get; }

    public ObservableCollection<string> CommonMistakes { get; }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool? isFinalAttempt;

    private async Task LoadQuickReferenceAsync()
    {
        try
        {
            IsBusy = true;
            var topics = await _tutorialContentService.GetTopicsAsync();
            var helpArticles = await _helpContentService.GetArticlesAsync();

            QuickReferences.Clear();

            foreach (var topic in topics.Take(3))
            {
                QuickReferences.Add(new QuickReferenceItem(topic.Title, $"Tutorial: {topic.Category}", topic.Steps.FirstOrDefault()?.Body ?? string.Empty));
            }

            foreach (var article in helpArticles.Take(3))
            {
                QuickReferences.Add(new QuickReferenceItem(article.Title, $"Help: {article.Category}", article.Content));
            }

            StatusMessage = QuickReferences.Count == 0
                ? "No quick reference content available."
                : "Quick reference loaded.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load quick reference: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public sealed record QuickReferenceItem(string Title, string Source, string Guidance);
}
