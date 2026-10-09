using Sergin.SharedKernel.Domain;

namespace Sergin.UserAccess.Domain.Authorization;

/// <summary>
/// Raised by every change to the scope tree, a role or an assignment — the one event the authorization audit
/// log is written from (BR-8). Who made the change and when is stamped where the entry is written, from the
/// sending user's context; the event names only what changed.
/// </summary>
public sealed record AuthorizationChangedDomainEvent(
    string Action,
    string EntityType,
    Guid EntityId,
    Guid? ScopeId,
    IReadOnlyDictionary<string, string?> Details) : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();

    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
