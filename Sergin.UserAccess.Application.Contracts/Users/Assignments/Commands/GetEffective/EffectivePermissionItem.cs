namespace Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetEffective;

public sealed record EffectivePermissionItem(Guid ScopeId, string ScopeName, bool IncludeSubtree, IReadOnlyCollection<string> Permissions);
