using Sergin.SharedKernel.Application.Commands.Queries;

namespace Sergin.UserAccess.Application.Contracts.Roles.Commands.GetAssignable;

/// <summary>FR-10: the roles visible and assignable at a scope: owned by it, or inheritable from an ancestor (BR-3).</summary>
public sealed record GetAssignableRolesQueryCommand(Guid ScopeId) : IQuery<IReadOnlyCollection<AssignableRoleItem>>;
