using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.SharedKernel.Application.Times;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts.Scopes;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Move;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Scopes.Commands.Move;

internal sealed class MoveScopeCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IScopeRepository scopes,
    IScopeAuthorization authorization,
    IAuthorizationCacheInvalidator cache,
    IDateTimeProvider clock) : ICommandHandler<MoveScopeCommand, MoveScopeCommandResponse>
{
    public async Task<ErrorOr<MoveScopeCommandResponse>> Handle(MoveScopeCommand request, CancellationToken cancellationToken)
    {
        // Both ends: managing the subtree where it is, and where it is going.
        if (!await authorization.CurrentUserHasAsync(ScopePermissions.Manage, request.Id, cancellationToken)
            || !await authorization.CurrentUserHasAsync(ScopePermissions.Manage, request.NewParentId, cancellationToken))
        {
            return Error.Forbidden();
        }

        Scope? scope = await scopes.GetAsync(new ScopeId(request.Id), cancellationToken);
        Scope? newParent = await scopes.GetAsync(new ScopeId(request.NewParentId), cancellationToken);

        if (scope is null || newParent is null)
        {
            return Error.NotFound();
        }

        IReadOnlyCollection<Scope> subtree = await scopes.GetSubtreeAsync(scope, cancellationToken);

        ErrorOr<Success> moved = scope.MoveUnder(newParent, subtree, clock.UtcNow);

        if (moved.IsError)
        {
            return moved.Errors;
        }

        // One save: the scope and every re-pathed descendant commit together or not at all (NFR-4), with the outbox
        // row that tells other modules to re-path their scoped records.
        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.Invalidate();

        return new MoveScopeCommandResponse(scope.Id.Value);
    }
}
