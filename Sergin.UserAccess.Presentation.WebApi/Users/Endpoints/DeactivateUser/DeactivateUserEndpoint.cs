using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Application.Dispatching;
using Sergin.UserAccess.Application.Users.Commands.DeactivateUser;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;

namespace Sergin.UserAccess.Presentation.WebApi.Users.Endpoints.DeactivateUser;

internal class DeactivateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPost("/users/{userId:guid}/deactivate", async ([FromRoute] Guid userId, ISerginSender sender) =>
            {
                ErrorOr<DeactivateUserCommandResponse> res = await sender.SendAsync(
                    new DeactivateUserCommand(userId));

                return res.ToApiResult();
            })
            .Produces<DeactivateUserCommandResponse>();
    }
}
