using Microsoft.Extensions.DependencyInjection;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Infrastructure.Authorization;

namespace Sergin.UserAccess.Authorization;

internal static class AuthorizationInstallationExtensions
{
    internal static IServiceCollection AddAuthorizationDependencies(this IServiceCollection services)
    {
        // One cache per process, invalidated through the same instance every write handler reaches.
        services.AddSingleton<EffectiveGrantCache>();
        services.AddSingleton<IAuthorizationCacheInvalidator>(provider => provider.GetRequiredService<EffectiveGrantCache>());

        // SharedKernel registers a deny-all checker with TryAdd after the module loop; this one wins.
        services.AddScoped<ScopedPermissionChecker>();
        services.AddScoped<IScopedPermissionChecker>(provider => provider.GetRequiredService<ScopedPermissionChecker>());
        services.AddTransient<IEffectiveGrantQueryRepository, EffectiveGrantQueryRepository>();

        return services;
    }
}
