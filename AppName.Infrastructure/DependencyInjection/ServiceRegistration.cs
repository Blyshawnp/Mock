using AppName.Core.Interfaces.Services;
using AppName.Core.Services.Rules;
using AppName.Core.Services.State;
using AppName.Core.Services.Validation;
using AppName.Infrastructure.AI;
using AppName.Infrastructure.Configuration;
using AppName.Infrastructure.Content;
using AppName.Infrastructure.Logging;
using AppName.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AppName.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddAppNameCoreInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEvaluationRulesService, EvaluationRulesService>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<ILogService, FileLogService>();
        services.AddSingleton<ISessionRepository, SqliteSessionRepository>();
        services.AddSingleton<ISessionStateService, SessionStateService>();
        services.AddSingleton<ILookupTableService, JsonLookupTableService>();
        services.AddSingleton<ISettingsService, JsonSettingsService>();
        services.AddSingleton<IHelpContentService, JsonHelpContentService>();
        services.AddSingleton<ITutorialContentService, JsonTutorialContentService>();
        services.AddSingleton<ISummaryService, FallbackSummaryService>();
        services.AddSingleton<IDiscordSystemMessageService, JsonDiscordSystemMessageService>();

        return services;
    }
}
