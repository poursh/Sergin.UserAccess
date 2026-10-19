namespace Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;

public sealed record ScopeTreeItem(Guid Id, Guid? ParentId, string Name, int Depth);
