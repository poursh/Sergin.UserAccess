using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Rename;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.Rename;

internal class RenameScopeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPut("/scopes/{scopeId:guid}/name", async (
            [FromRoute] Guid scopeId, [FromBody] RenameScopeModel scope, ISender sender) =>
        {
            ErrorOr<RenameScopeCommandResponse> res = await sender.Send(new RenameScopeCommand(scopeId, new ScopeName(scope.Name)));

            return res.ToApiResult();
        });
    }
}
