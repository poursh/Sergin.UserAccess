namespace Sergin.UserAccess.Application.Contracts.Roles.Commands.GetOne;

public sealed record RoleQueryResponse(
    Guid Id, Guid OwnerScopeId, string OwnerScopeName, string Name, bool IsInheritable, IReadOnlyCollection<string> Permissions);
