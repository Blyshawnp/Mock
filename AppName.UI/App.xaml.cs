using AppName.Infrastructure.DependencyInjection;
using AppName.UI.Services;
using AppName.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AppName.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    try
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddAppNameCoreInfrastructure();
        serviceCollection.AddAppNameUi();
        serviceCollection.AddTransient<MainWindow>();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            ex.ToString(),
            "Startup Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
