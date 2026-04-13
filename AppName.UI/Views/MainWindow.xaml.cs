using AppName.UI.ViewModels;
using System.Windows;

namespace AppName.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(
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

        CallsViewHost.DataContext = callsViewModel;
        SupervisorTransferViewHost.DataContext = supervisorTransferViewModel;
        ReviewViewHost.DataContext = reviewViewModel;
        SetupWizardViewHost.DataContext = setupWizardViewModel;
        TutorialViewHost.DataContext = tutorialViewModel;
        HelpViewHost.DataContext = helpViewModel;
        NewbieShiftViewHost.DataContext = newbieShiftViewModel;
        SettingsViewHost.DataContext = settingsViewModel;
    }
}
