using Sergin.SharedKernel.Domain;

namespace Sergin.UserAccess.Domain.Scopes;

/// <summary>
/// A node of the scope tree (company, region, feeder, …). Roles belong to a scope and are assigned to users
/// at a scope. The tree has exactly one root, seeded by the migration with <see cref="RootId"/>; every other
/// scope has one parent. <see cref="Path"/> is the materialised chain of ids from the root down to this scope,
/// stored as a Postgres <c>ltree</c>, so ancestor and descendant questions are one indexed comparison.
/// </summary>
public class Scope : AggregateRoot<ScopeId>
{
    private Scope() { }

    /// <summary>The id the migration seeds the root scope with.</summary>
    public static ScopeId RootId { get; } = new(Guid.Parse("01920000-0000-7000-8000-0000000000b1"));

    public ScopeName Name { get; private set; }

    public ScopeId? ParentId { get; private set; }

    public ScopePath Path { get; private set; }

    public bool IsRoot => ParentId is null;

    /// <summary>
    /// The root, as the migration seeds it. Internal: the application never creates a second root; this exists
    /// so a tree can be built in memory.
    /// </summary>
    internal static Scope CreateRoot(ScopeName name)
    {
        return new Scope
        {
            Id = RootId,
            Name = name,
            Path = ScopePath.Root(RootId)
        };
    }

    public static Scope Create(ScopeName name, Scope parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        var id = new ScopeId(Guid.CreateVersion7());

        Scope scope = new()
        {
            Id = id,
            Name = name,
            ParentId = parent.Id,
            Path = parent.Path.Child(id)
        };

        return scope;
    }

    public void Rename(ScopeName name)
    {
        Name = name;
    }

    /// <summary>
    /// Moves this scope, with its whole subtree, under <paramref name="newParent"/>. Refuses moving the root and
    /// moving a scope under itself or one of its descendants. <paramref name="subtree"/> must be every scope
    /// below this one (loaded through <c>IScopeRepository.GetSubtreeAsync</c>): their paths are rewritten here,
    /// in memory, so the whole move is one save and one transaction (NFR-4). Whether the move would strand an
    /// assignment outside its role's owner subtree (BR-6) is not visible from inside this aggregate; the move
    /// command's validator checks it.
    /// </summary>
    public ErrorOr<Success> MoveUnder(Scope newParent, IReadOnlyCollection<Scope> subtree)
    {
        ArgumentNullException.ThrowIfNull(newParent);
        ArgumentNullException.ThrowIfNull(subtree);

        if (IsRoot)
        {
            return Error.Validation(nameof(ParentId), "The root scope cannot be moved.");
        }

        if (Path.IsAncestorOrSelfOf(newParent.Path))
        {
            return Error.Validation(nameof(ParentId), "A scope cannot be moved under itself or one of its descendants.");
        }

        if (newParent.Id == ParentId)
        {
            return Result.Success;
        }

        ScopePath from = Path;
        ScopePath to = newParent.Path.Child(Id);

        foreach (Scope descendant in subtree.Where(scope => scope.Id != Id))
        {
            descendant.Path = descendant.Path.Rebase(from, to);
        }

        ParentId = newParent.Id;
        Path = to;

        return Result.Success;
    }
}

public sealed record ScopeId(Guid Value)
{
    /// <summary>This id as one <c>ltree</c> label: 32 hex digits, no hyphens.</summary>
    public string Label => Value.ToString("N");
}

public sealed record ScopeName(string Value)
{
    public const int MaxLength = 200;
}

/// <summary>
/// The dot-separated chain of <see cref="ScopeId.Label"/>s from the root to a scope, root first. The same
/// string Postgres stores as <c>ltree</c>, so <see cref="IsAncestorOrSelfOf"/> here and <c>@&gt;</c> in SQL agree.
/// </summary>
public sealed record ScopePath(string Value)
{
    private const char Separator = '.';

    /// <summary>Levels from the root, the root being 1: Postgres' <c>nlevel</c>.</summary>
    public int Depth => Value.Count(character => character == Separator) + 1;

    public static ScopePath Root(ScopeId rootId)
    {
        ArgumentNullException.ThrowIfNull(rootId);

        return new ScopePath(rootId.Label);
    }

    public ScopePath Child(ScopeId childId)
    {
        ArgumentNullException.ThrowIfNull(childId);

        return new ScopePath($"{Value}{Separator}{childId.Label}");
    }

    /// <summary>True when this path is <paramref name="other"/> or one of its ancestors: Postgres' <c>@&gt;</c>.</summary>
    public bool IsAncestorOrSelfOf(ScopePath other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return other.Value == Value
            || other.Value.Length > Value.Length
                && other.Value[Value.Length] == Separator
                && other.Value.StartsWith(Value, StringComparison.Ordinal);
    }

    /// <summary>This path with the prefix <paramref name="from"/> replaced by <paramref name="to"/>.</summary>
    public ScopePath Rebase(ScopePath from, ScopePath to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (!from.IsAncestorOrSelfOf(this))
        {
            throw new ArgumentException($"'{Value}' is not below '{from.Value}'.", nameof(from));
        }

        return new ScopePath(to.Value + Value[from.Value.Length..]);
    }
}
