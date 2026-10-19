using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.Create;
using Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.Rename;
using Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.SetInheritable;
using Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.GrantPermission;
using Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.RevokePermission;
using Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.Delete;
using Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.GetOne;
using Sergin.UserAccess.Application.Roles;
using Sergin.UserAccess.Domain.Roles;
using Sergin.UserAccess.Infrastructure.Roles.Repositories;
using Sergin.UserAccess.Infrastructure.Roles.Repositories.Queries;

namespace Sergin.UserAccess.Roles;

internal static class RoleInstallationExtensions
{
    internal static IServiceCollection AddRoleDependencies(this IServiceCollection services)
    {
        services.AddTransient<IRoleRepository, RoleRepository>();
        services.AddTransient<IRoleQueryRepository, RoleQueryRepository>();

        return services;
    }

    internal static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        new CreateRoleEndpoint().MapEndpoint(routeBuilder);
        new RenameRoleEndpoint().MapEndpoint(routeBuilder);
        new SetRoleInheritableEndpoint().MapEndpoint(routeBuilder);
        new GrantRolePermissionEndpoint().MapEndpoint(routeBuilder);
        new RevokeRolePermissionEndpoint().MapEndpoint(routeBuilder);
        new DeleteRoleEndpoint().MapEndpoint(routeBuilder);
        new GetRoleEndpoint().MapEndpoint(routeBuilder);

        return routeBuilder;
    }
}
