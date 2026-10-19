using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Domain.Securities;

namespace Sergin.UserAccess.Application.Contracts.Roles.Commands.GrantPermission;

public sealed record GrantRolePermissionCommand(Guid Id, Permission Permission) : ICommand<GrantRolePermissionCommandResponse>;
