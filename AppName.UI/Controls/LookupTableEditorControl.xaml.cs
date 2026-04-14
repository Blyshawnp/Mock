using AppName.Core.Models.Lookup;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AppName.UI.Controls;

/// <summary>
/// Dedicated editor for LookupItem collections only.
/// Rich-model editors (ShowOfferSeed, DonorProfileSeed) stay in SettingsView.
/// </summary>
public partial class LookupTableEditorControl : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(LookupTableEditorControl),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(ObservableCollection<LookupItem>),
        typeof(LookupTableEditorControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(LookupItem),
        typeof(LookupTableEditorControl),
        new PropertyMetadata(null));

    public LookupTableEditorControl()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ObservableCollection<LookupItem>? ItemsSource
    {
        get => (ObservableCollection<LookupItem>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public LookupItem? SelectedItem
    {
        get => (LookupItem?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private void AddButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is null)
        {
            return;
        }

        var seed = ItemsSource.FirstOrDefault();

        var newItem = new LookupItem
        {
            Category = seed?.Category ?? Title.Replace(" ", string.Empty),
            Subcategory = seed?.Subcategory ?? string.Empty,
            Name = "New Item",
            Description = string.Empty,
            IsEnabled = true,
            SortOrder = ItemsSource.Count + 1
        };

        ItemsSource.Add(newItem);
        SelectedItem = newItem;
        NormalizeSortOrder();
    }

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is null || SelectedItem is null)
        {
            return;
        }

        ItemsSource.Remove(SelectedItem);
        NormalizeSortOrder();
    }

    private void MoveUpButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is null || SelectedItem is null)
        {
            return;
        }

        var index = ItemsSource.IndexOf(SelectedItem);
        if (index <= 0)
        {
            return;
        }

        ItemsSource.Move(index, index - 1);
        NormalizeSortOrder();
    }

    private void MoveDownButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ItemsSource is null || SelectedItem is null)
        {
            return;
        }

        var index = ItemsSource.IndexOf(SelectedItem);
        if (index < 0 || index >= ItemsSource.Count - 1)
        {
            return;
        }

        ItemsSource.Move(index, index + 1);
        NormalizeSortOrder();
    }

    private void NormalizeSortOrder()
    {
        if (ItemsSource is null)
        {
            return;
        }

        for (var index = 0; index < ItemsSource.Count; index++)
        {
            ItemsSource[index].SortOrder = index + 1;
        }

        EditorGrid.Items.Refresh();
    }
}
