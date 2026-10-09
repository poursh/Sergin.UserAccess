using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Move;

namespace Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.Move;

internal class MoveScopeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/scopes/{scopeId:guid}/move", async (
            [FromRoute] Guid scopeId, [FromBody] MoveScopeModel move, ISender sender) =>
        {
            ErrorOr<MoveScopeCommandResponse> res = await sender.Send(new MoveScopeCommand(scopeId, move.NewParentId));

            return res.ToApiResult();
        });
    }
}
