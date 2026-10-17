using Sergin.SharedKernel.Application.Events.Integration;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Scopes.Commands.Move;

/// <summary>
/// Publishes a move to every module with scoped aggregates, whose records keep a copy of their scope's path. The
/// outbox row rides the move's own save, so the paths follow only a move that committed.
/// </summary>
internal sealed class ScopesMovedTranslator : IIntegrationEventTranslator<ScopesMovedDomainEvent>
{
    public IIntegrationEvent Translate(ScopesMovedDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return new ScopesMovedIntegrationEvent(domainEvent.Id, domainEvent.OccurredOnUtc, domainEvent.ScopeIds);
    }
}
