using GitAssistant.Core.Interface;
using GitAssistant.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GitAssistant.Core;

public static class ApplicationDI
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IGitCommandRunner, GitCommandRunner>();

        services.AddSingleton<IGitLogService, GitLogService>();

        services.AddSingleton<IGitEngine, GitEngine>();

        return services;
    }
}
