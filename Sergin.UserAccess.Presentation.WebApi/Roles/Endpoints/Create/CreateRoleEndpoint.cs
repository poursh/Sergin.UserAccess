using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Sergin.SharedKernel.Presentation.WebApi.Endpoints.Results;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Create;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Presentation.WebApi.Roles.Endpoints.Create;

internal class CreateRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder
            .MapPost("/roles", async ([FromBody] NewRoleModel role, ISender sender) =>
            {
                List<Permission> permissions = [];

                foreach (string value in role.Permissions)
                {
                    if (!Permission.TryCreate(value, out Permission? permission))
                    {
                        return Results.ValidationProblem(new Dictionary<string, string[]>
                        {
                            [nameof(NewRoleModel.Permissions)] = [$"'{value}' is not a valid permission."]
                        });
                    }

                    permissions.Add(permission);
                }

                ErrorOr<CreateRoleCommandResponse> res = await sender.Send(
                    new CreateRoleCommand(role.OwnerScopeId, new RoleName(role.Name), role.IsInheritable, permissions));

                return res.ToApiResult();
            })
            .Produces<CreateRoleCommandResponse>();
    }
}
