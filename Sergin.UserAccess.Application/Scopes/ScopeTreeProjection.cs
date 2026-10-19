using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Scopes;

/// <summary>
/// Narrows the scope tree to the scopes a predicate shows, keeping its order. A shown scope whose parent is not
/// shown comes back as a top-level node, so a caller who reaches a region but not the company sees the region as a root.
/// </summary>
/// <remarks>Shared by the tree query and the scope picker's directory, so the two never disagree on the shape.</remarks>
internal static class ScopeTreeProjection
{
    internal static IReadOnlyList<VisibleScope> Project(IReadOnlyCollection<ScopeTreeRow> rows, Func<ScopePath, bool> visible)
    {
        HashSet<Guid> shown = [.. rows.Where(row => visible(new ScopePath(row.Path))).Select(row => row.Id)];

        return
        [
            .. rows
                .Where(row => shown.Contains(row.Id))
                .Select(row => new VisibleScope(
                    row.Id,
                    row.ParentId is { } parentId && shown.Contains(parentId) ? parentId : null,
                    row.Name,
                    new ScopePath(row.Path).Depth))
        ];
    }
}

internal sealed record VisibleScope(Guid Id, Guid? ParentId, string Name, int Depth);
