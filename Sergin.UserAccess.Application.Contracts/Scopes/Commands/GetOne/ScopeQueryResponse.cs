namespace Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetOne;

public sealed record ScopeQueryResponse(Guid Id, Guid? ParentId, string Name, string? ParentName);
