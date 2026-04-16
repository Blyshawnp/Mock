using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppName.UI.Controls;

public partial class SummaryPanelControl : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(SummaryPanelControl),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SummaryTextProperty = DependencyProperty.Register(
        nameof(SummaryText),
        typeof(string),
        typeof(SummaryPanelControl),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty RegenerateCommandProperty = DependencyProperty.Register(
        nameof(RegenerateCommand),
        typeof(ICommand),
        typeof(SummaryPanelControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RetryCommandProperty = DependencyProperty.Register(
        nameof(RetryCommand),
        typeof(ICommand),
        typeof(SummaryPanelControl),
        new PropertyMetadata(null));

    public SummaryPanelControl()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string SummaryText
    {
        get => (string)GetValue(SummaryTextProperty);
        set => SetValue(SummaryTextProperty, value);
    }

    public ICommand? RegenerateCommand
    {
        get => (ICommand?)GetValue(RegenerateCommandProperty);
        set => SetValue(RegenerateCommandProperty, value);
    }

    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }
}
