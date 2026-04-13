using System.Windows;
using System.Windows.Controls;

namespace AppName.UI.Controls;

public partial class OtherTextInputControl : UserControl
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label),
        typeof(string),
        typeof(OtherTextInputControl),
        new PropertyMetadata("Other details"));

    public static readonly DependencyProperty TextValueProperty = DependencyProperty.Register(
        nameof(TextValue),
        typeof(string),
        typeof(OtherTextInputControl),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsRequiredMessageVisibleProperty = DependencyProperty.Register(
        nameof(IsRequiredMessageVisible),
        typeof(Visibility),
        typeof(OtherTextInputControl),
        new PropertyMetadata(Visibility.Collapsed));

    public OtherTextInputControl()
    {
        InitializeComponent();
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string TextValue
    {
        get => (string)GetValue(TextValueProperty);
        set => SetValue(TextValueProperty, value);
    }

    public Visibility IsRequiredMessageVisible
    {
        get => (Visibility)GetValue(IsRequiredMessageVisibleProperty);
        set => SetValue(IsRequiredMessageVisibleProperty, value);
    }
}
