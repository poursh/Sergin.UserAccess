namespace Sergin.UserAccess.Application.Contracts.Roles.Commands.GetAssignable;

public sealed record AssignableRoleItem(Guid Id, string Name, Guid OwnerScopeId, string OwnerScopeName, bool IsInheritable);
