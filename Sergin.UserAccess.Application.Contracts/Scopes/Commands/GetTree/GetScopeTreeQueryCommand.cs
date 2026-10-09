using Sergin.SharedKernel.Application.Commands.Queries;

namespace Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetTree;

/// <summary>The live scopes the caller may read, each with its parent, root first and siblings by name.</summary>
public sealed record GetScopeTreeQueryCommand : IQuery<IReadOnlyCollection<ScopeTreeItem>>;
