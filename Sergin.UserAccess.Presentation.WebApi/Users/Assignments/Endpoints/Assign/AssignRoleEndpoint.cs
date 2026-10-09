using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.Assign;

namespace Sergin.UserAccess.Presentation.WebApi.Users.Assignments.Endpoints.Assign;

internal class AssignRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPost("/users/{userId:guid}/assignments", async (
                [FromRoute] Guid userId, [FromBody] NewAssignmentModel assignment, ISender sender) =>
            {
                ErrorOr<AssignRoleCommandResponse> res = await sender.Send(new AssignRoleCommand(
                    userId,
                    assignment.RoleId,
                    assignment.ScopeId,
                    assignment.IncludeSubtree,
                    assignment.ValidFromUtc,
                    assignment.ValidUntilUtc));

                return res.ToApiResult();
            })
            .Produces<AssignRoleCommandResponse>();
    }
}
