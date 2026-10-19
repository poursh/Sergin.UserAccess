using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Contracts.Roles.Commands.Create;

public sealed record CreateRoleCommand(
    Guid OwnerScopeId, RoleName Name, bool IsInheritable, IReadOnlyCollection<Permission> Permissions)
    : ICommand<CreateRoleCommandResponse>;
