using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;

namespace Sergin.UserAccess.Presentation.WebApi.Users.Assignments.Endpoints.GetList;

internal class GetUserAssignmentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapGet("/users/{userId:guid}/assignments", async ([FromRoute] Guid userId, ISender sender) =>
            {
                ErrorOr<IReadOnlyCollection<UserAssignmentItem>> res = await sender.Send(new GetUserAssignmentsQueryCommand(userId));

                return res.ToApiResult();
            })
            .Produces<IReadOnlyCollection<UserAssignmentItem>>();
    }
}
