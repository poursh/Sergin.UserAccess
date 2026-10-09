using Sergin.SharedKernel.Domain;

namespace Sergin.UserAccess.Domain.Authorization;

/// <summary>
/// One row of the authorization audit log (BR-8, NFR-6): who changed what in the scope tree, a role or an
/// assignment, and when. Append-only: nothing in the application updates or deletes an entry, and a database
/// trigger refuses it from anywhere else.
/// </summary>
public sealed class AuthorizationAuditEntry : Entity<Guid>
{
    private AuthorizationAuditEntry() { }

    public DateTime OccurredAtUtc { get; private set; }

    public Guid ActorId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public Guid? ScopeId { get; private set; }

    /// <summary>The change's specifics as a JSON object, stored as <c>jsonb</c>.</summary>
    public string Details { get; private set; } = "{}";

    public static AuthorizationAuditEntry Record(AuthorizationChangedDomainEvent change, Guid actorId, string detailsJson)
    {
        ArgumentNullException.ThrowIfNull(change);

        return new AuthorizationAuditEntry
        {
            Id = change.Id,
            OccurredAtUtc = change.OccurredOnUtc,
            ActorId = actorId,
            Action = change.Action,
            EntityType = change.EntityType,
            EntityId = change.EntityId,
            ScopeId = change.ScopeId,
            Details = detailsJson
        };
    }
}
