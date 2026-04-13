using AppName.Core.Interfaces.Services;
using AppName.Core.Models.Help;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AppName.UI.ViewModels;

public partial class HelpViewModel : ViewModelBase
{
    private readonly IHelpContentService _helpContentService;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private List<HelpArticle> _allArticles = [];

    public HelpViewModel(IHelpContentService helpContentService)
    {
        _helpContentService = helpContentService;

        Categories = [];
        Articles = [];

        _ = EnsureLoadedAsync();
    }

    public ObservableCollection<string> Categories { get; }

    public ObservableCollection<HelpArticle> Articles { get; }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedCategory = "All";

    [ObservableProperty]
    private HelpArticle? selectedArticle;

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            IsBusy = true;
            _allArticles = (await _helpContentService.GetArticlesAsync(cancellationToken)).ToList();

            Categories.Clear();
            Categories.Add("All");
            foreach (var category in _allArticles
                         .Select(x => x.Category)
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(x => x))
            {
                Categories.Add(category);
            }

            ApplyFilters();
            StatusMessage = Articles.Count == 0 ? "No help articles found." : "Help content loaded.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Help loading failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            _loadLock.Release();
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();

    partial void OnSelectedCategoryChanged(string value) => ApplyFilters();

    private void ApplyFilters()
    {
        var query = (_allArticles ?? [])
            .Where(x => string.Equals(SelectedCategory, "All", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(x.Category, SelectedCategory, StringComparison.OrdinalIgnoreCase))
            .Where(x => string.IsNullOrWhiteSpace(SearchText)
                        || x.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                        || x.Content.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                        || x.Tags.Any(t => t.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Title)
            .ToList();

        Articles.Clear();
        foreach (var article in query)
        {
            Articles.Add(article);
        }

        SelectedArticle = Articles.FirstOrDefault();
    }
}
