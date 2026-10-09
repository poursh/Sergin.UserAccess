using Sergin.SharedKernel.Application.Commands;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Contracts.Roles.Commands.Rename;

public sealed record RenameRoleCommand(Guid Id, RoleName Name) : ICommand<RenameRoleCommandResponse>;
