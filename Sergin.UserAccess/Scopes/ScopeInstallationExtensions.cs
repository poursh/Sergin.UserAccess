using Microsoft.Extensions.DependencyInjection;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Infrastructure.Scopes.Repositories;

namespace Sergin.UserAccess.Scopes;

internal static class ScopeInstallationExtensions
{
    internal static IServiceCollection AddScopeDependencies(this IServiceCollection services)
    {
        services.AddTransient<IScopeRepository, ScopeRepository>();

        return services;
    }
}
