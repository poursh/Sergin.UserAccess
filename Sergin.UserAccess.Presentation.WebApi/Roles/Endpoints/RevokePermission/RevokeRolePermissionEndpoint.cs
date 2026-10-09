using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.RevokePermission;
using Sergin.SharedKernel.Domain.Securities;

namespace Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.RevokePermission;

internal class RevokeRolePermissionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapDelete("/roles/{roleId:guid}/permissions/{permission}", async (
            [FromRoute] Guid roleId, [FromRoute] string permission, ISender sender) =>
        {
            if (!Permission.TryCreate(permission, out Permission? parsed))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Permission"] = [$"'{permission}' is not a valid permission."]
                });
            }

            ErrorOr<RevokeRolePermissionCommandResponse> res = await sender.Send(new RevokeRolePermissionCommand(roleId, parsed));

            return res.ToApiResult();
        });
    }
}
