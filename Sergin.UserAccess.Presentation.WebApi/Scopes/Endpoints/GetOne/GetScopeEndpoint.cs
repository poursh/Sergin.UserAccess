using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetOne;

namespace Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.GetOne;

internal class GetScopeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/scopes/{scopeId:guid}", async ([FromRoute] Guid scopeId, ISender sender) =>
        {
            ErrorOr<ScopeQueryResponse> res = await sender.Send(new GetScopeByIdQueryCommand(scopeId));

            return res.ToApiResult();
        });
    }
}
