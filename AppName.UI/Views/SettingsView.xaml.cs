using AppName.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AppName.UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private async void SettingsView_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && vm.InitializeCommand.CanExecute(null))
        {
            await vm.InitializeCommand.ExecuteAsync(null);
        }
    }
}
