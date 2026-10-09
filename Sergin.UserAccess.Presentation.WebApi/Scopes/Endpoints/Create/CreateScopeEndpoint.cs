using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Create;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.Create;

internal class CreateScopeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPost("/scopes", async ([FromBody] NewScopeModel scope, ISender sender) =>
            {
                ErrorOr<CreateScopeCommandResponse> res = await sender.Send(
                    new CreateScopeCommand(scope.ParentId, new ScopeName(scope.Name)));

                return res.ToApiResult();
            })
            .Produces<CreateScopeCommandResponse>();
    }
}
