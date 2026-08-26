using Microsoft.Extensions.DependencyInjection;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Infrastructure.Roles.Repositories;

namespace Sergin.UserAccess.Roles;

internal static class RoleInstallationExtensions
{
    internal static IServiceCollection AddRoleDependencies(this IServiceCollection services)
    {
        services.AddTransient<IRoleRepository, RoleRepository>();

        return services;
    }
}
