using System.Text.Json;
using Sergin.SharedKernel.Application.Events;
using Sergin.SharedKernel.Application.Securities.Users;
using Sergin.UserAccess.Domain.Authorization;

namespace Sergin.UserAccess.Application.Authorization;

/// <summary>
/// Writes every authorization change to the audit log (BR-8, NFR-6). Runs inside the originating save, on the
/// same DbContext, so the entry commits with the change it records or not at all. The actor is the sending
/// user's context — the same identity the audit stamps take.
/// </summary>
internal sealed class AuthorizationAuditDomainEventHandler(IUserContext userContext, IAuthorizationAuditLog auditLog)
    : IDomainEventHandler<AuthorizationChangedDomainEvent>
{
    public Task Handle(AuthorizationChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        string details = JsonSerializer.Serialize(domainEvent.Details);
        auditLog.Append(AuthorizationAuditEntry.Record(domainEvent, userContext.Id.Value, details));

        return Task.CompletedTask;
    }
}
