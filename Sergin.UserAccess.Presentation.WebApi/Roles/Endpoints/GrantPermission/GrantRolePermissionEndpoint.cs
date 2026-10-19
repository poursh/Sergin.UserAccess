using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GrantPermission;
using Sergin.SharedKernel.Domain.Securities;

namespace Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.GrantPermission;

internal class GrantRolePermissionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("/roles/{roleId:guid}/permissions", async (
            [FromRoute] Guid roleId, [FromBody] RolePermissionModel grant, ISender sender) =>
        {
            if (!Permission.TryCreate(grant.Permission, out Permission? permission))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(RolePermissionModel.Permission)] = [$"'{grant.Permission}' is not a valid permission."]
                });
            }

            ErrorOr<GrantRolePermissionCommandResponse> res = await sender.Send(new GrantRolePermissionCommand(roleId, permission));

            return res.ToApiResult();
        });
    }
}
