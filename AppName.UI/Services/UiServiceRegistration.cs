using AppName.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AppName.UI.Services;

public static class UiServiceRegistration
{
    public static IServiceCollection AddAppNameUi(this IServiceCollection services)
    {
        services.AddSingleton<IAudioFeedbackService, AudioFeedbackService>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<CallsViewModel>();
        services.AddTransient<SupervisorTransferViewModel>();
        services.AddTransient<SupervisorTransferOnlyDialogViewModel>();
        services.AddTransient<ReviewViewModel>();
        services.AddTransient<SetupWizardViewModel>();
        services.AddTransient<TutorialViewModel>();
        services.AddTransient<HelpViewModel>();
        services.AddTransient<NewbieShiftViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services;
    }
}
