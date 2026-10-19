using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Delete;

namespace Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.Delete;

internal class DeleteRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapDelete("/roles/{roleId:guid}", async (
            [FromRoute] Guid roleId, [FromQuery] bool? cascade, ISender sender) =>
        {
            ErrorOr<Deleted> res = await sender.Send(new DeleteRoleCommand(roleId, cascade ?? false));

            return res.ToApiResult();
        });
    }
}
