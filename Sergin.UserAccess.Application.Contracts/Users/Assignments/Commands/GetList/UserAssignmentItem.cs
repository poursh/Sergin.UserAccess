namespace Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;

public sealed record UserAssignmentItem(
    Guid Id,
    Guid RoleId,
    string RoleName,
    Guid ScopeId,
    string ScopeName,
    bool IncludeSubtree,
    DateTime? ValidFromUtc,
    DateTime? ValidUntilUtc,
    DateTime? RevokedAtUtc);
