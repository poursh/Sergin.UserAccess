using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Rename;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.Rename;

internal class RenameRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPut("/roles/{roleId:guid}/name", async (
            [FromRoute] Guid roleId, [FromBody] RenameRoleModel role, ISender sender) =>
        {
            ErrorOr<RenameRoleCommandResponse> res = await sender.Send(new RenameRoleCommand(roleId, new RoleName(role.Name)));

            return res.ToApiResult();
        });
    }
}
