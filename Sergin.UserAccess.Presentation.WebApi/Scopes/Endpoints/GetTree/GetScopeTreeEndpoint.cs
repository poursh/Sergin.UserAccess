using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;

namespace Sergin.UserAccess.Presentation.WebApi.Scopes.Endpoints.GetTree;

internal class GetScopeTreeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet("/scopes", async (ISender sender) =>
            {
                ErrorOr<IReadOnlyCollection<ScopeTreeItem>> res = await sender.Send(new GetScopeTreeQueryCommand());

                return res.ToApiResult();
            })
            .Produces<IReadOnlyCollection<ScopeTreeItem>>();
    }
}
