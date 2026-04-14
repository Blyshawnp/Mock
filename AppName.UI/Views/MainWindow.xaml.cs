using AppName.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppName.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(
        HomeViewModel homeViewModel,
        CallsViewModel callsViewModel,
        SupervisorTransferViewModel supervisorTransferViewModel,
        ReviewViewModel reviewViewModel,
        SetupWizardViewModel setupWizardViewModel,
        TutorialViewModel tutorialViewModel,
        HelpViewModel helpViewModel,
        NewbieShiftViewModel newbieShiftViewModel,
        SettingsViewModel settingsViewModel)
    {
        InitializeComponent();

        HomeViewHost.DataContext = homeViewModel;
        CallsViewHost.DataContext = callsViewModel;
        SupervisorTransferViewHost.DataContext = supervisorTransferViewModel;
        ReviewViewHost.DataContext = reviewViewModel;
        SetupWizardViewHost.DataContext = setupWizardViewModel;
        TutorialViewHost.DataContext = tutorialViewModel;
        HelpViewHost.DataContext = helpViewModel;
        SettingsViewHost.DataContext = settingsViewModel;

        _ = newbieShiftViewModel;
    }

    private void NavButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (int.TryParse(button.Tag?.ToString(), out var tabIndex))
        {
            ContentTabs.SelectedIndex = tabIndex;
        }

        HighlightSelectedNav(button);
    }

    private static void HighlightSelectedNav(Button selected)
    {
        if (selected.Parent is not Panel panel)
        {
            return;
        }

        foreach (var child in panel.Children)
        {
            if (child is not Button nav)
            {
                continue;
            }

            nav.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0015223A"));
            nav.BorderBrush = Brushes.Transparent;
        }

        selected.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE34468"));
        selected.BorderBrush = Brushes.Transparent;
    }

    private void ExitButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
