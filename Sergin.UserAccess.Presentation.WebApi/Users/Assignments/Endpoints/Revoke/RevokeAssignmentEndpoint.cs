using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.Revoke;

namespace Sergin.UserAccess.Presentation.WebApi.Users.Assignments.Endpoints.Revoke;

internal class RevokeAssignmentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapDelete("/users/{userId:guid}/assignments/{assignmentId:guid}", async (
            [FromRoute] Guid userId, [FromRoute] Guid assignmentId, ISender sender) =>
        {
            ErrorOr<RevokeAssignmentCommandResponse> res = await sender.Send(new RevokeAssignmentCommand(userId, assignmentId));

            return res.ToApiResult();
        });
    }
}
