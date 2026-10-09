using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Domain.Securities;

namespace Sergin.UserAccess.Application.Contracts.Roles.Commands.RevokePermission;

public sealed record RevokeRolePermissionCommand(Guid Id, Permission Permission) : ICommand<RevokeRolePermissionCommandResponse>;
