using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Delete;

namespace Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.Delete;

internal class DeleteScopeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapDelete("/scopes/{scopeId:guid}", async (
            [FromRoute] Guid scopeId, [FromQuery] bool? cascade, ISender sender) =>
        {
            ErrorOr<Deleted> res = await sender.Send(new DeleteScopeCommand(scopeId, cascade ?? false));

            return res.ToApiResult();
        });
    }
}
