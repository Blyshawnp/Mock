using AppName.Core.Models.Enums;
using System.Windows;
using System.Windows.Controls;

namespace AppName.UI.Controls;

public partial class PassFailToggleControl : UserControl
{
    public static readonly DependencyProperty OutcomeProperty = DependencyProperty.Register(
        nameof(Outcome),
        typeof(CallOutcome),
        typeof(PassFailToggleControl),
        new FrameworkPropertyMetadata(
            CallOutcome.NotScored,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnOutcomeChanged));

    public PassFailToggleControl()
    {
        InitializeComponent();
        SyncSelection();
    }

    public CallOutcome Outcome
    {
        get => (CallOutcome)GetValue(OutcomeProperty);
        set => SetValue(OutcomeProperty, value);
    }

    private static void OnOutcomeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PassFailToggleControl control)
        {
            control.SyncSelection();
        }
    }

    private void PassOption_OnChecked(object sender, RoutedEventArgs e)
    {
        Outcome = CallOutcome.Pass;
    }

    private void FailOption_OnChecked(object sender, RoutedEventArgs e)
    {
        Outcome = CallOutcome.Fail;
    }

    private void NotScoredOption_OnChecked(object sender, RoutedEventArgs e)
    {
        Outcome = CallOutcome.NotScored;
    }

    private void SyncSelection()
    {
        if (PassOption is null || FailOption is null || NotScoredOption is null)
        {
            return;
        }

        PassOption.IsChecked = Outcome == CallOutcome.Pass;
        FailOption.IsChecked = Outcome == CallOutcome.Fail;
        NotScoredOption.IsChecked = Outcome == CallOutcome.NotScored;
    }
}
