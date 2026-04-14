using AppName.Core.Models.Common;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AppName.UI.Controls;

public partial class WarningBannerControl : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IReadOnlyList<AppWarning>),
        typeof(WarningBannerControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty BannerVisibilityProperty = DependencyProperty.Register(
        nameof(BannerVisibility),
        typeof(Visibility),
        typeof(WarningBannerControl),
        new PropertyMetadata(Visibility.Collapsed));

    public WarningBannerControl()
    {
        InitializeComponent();
    }

    public IReadOnlyList<AppWarning>? ItemsSource
    {
        get => (IReadOnlyList<AppWarning>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public Visibility BannerVisibility
    {
        get => (Visibility)GetValue(BannerVisibilityProperty);
        set => SetValue(BannerVisibilityProperty, value);
    }
}
