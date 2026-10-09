using Sergin.SharedKernel.Application.Commands;

namespace Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.Assign;

public sealed record AssignRoleCommand(
    Guid UserId, Guid RoleId, Guid ScopeId, bool IncludeSubtree, DateTime? ValidFromUtc, DateTime? ValidUntilUtc)
    : ICommand<AssignRoleCommandResponse>;
