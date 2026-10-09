using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.Create;
using Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.Rename;
using Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.Move;
using Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.Delete;
using Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.GetOne;
using Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.GetTree;
using Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.GetAssignableRoles;
using Sergin.UserAccess.Application.Scopes;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Infrastructure.Scopes.Repositories;
using Sergin.UserAccess.Infrastructure.Scopes.Repositories.Queries;

namespace Sergin.UserAccess.Scopes;

internal static class ScopeInstallationExtensions
{
    internal static IServiceCollection AddScopeDependencies(this IServiceCollection services)
    {
        services.AddTransient<IScopeRepository, ScopeRepository>();
        services.AddTransient<IScopeQueryRepository, ScopeQueryRepository>();

        return services;
    }

    internal static IEndpointRouteBuilder MapScopeEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        new CreateScopeEndpoint().MapEndpoint(routeBuilder);
        new RenameScopeEndpoint().MapEndpoint(routeBuilder);
        new MoveScopeEndpoint().MapEndpoint(routeBuilder);
        new DeleteScopeEndpoint().MapEndpoint(routeBuilder);
        new GetScopeEndpoint().MapEndpoint(routeBuilder);
        new GetScopeTreeEndpoint().MapEndpoint(routeBuilder);
        new GetAssignableRolesEndpoint().MapEndpoint(routeBuilder);

        return routeBuilder;
    }
}
