using Sergin.SharedKernel.Application.Concurrency;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetOne;

namespace Sergin.UserAccess.Application.Scopes.Commands.GetOne;

internal sealed class GetScopeByIdQueryCommandHandler(
    IScopeQueryRepository repository, IScopeAuthorization authorization, ConcurrencyContext concurrency)
    : VersionedQueryHandler<GetScopeByIdQueryCommand, ScopeQueryResponse>(concurrency)
{
    public override async Task<ErrorOr<Versioned<ScopeQueryResponse>>> HandleVersioned(
        GetScopeByIdQueryCommand request, CancellationToken cancellationToken)
    {
        if (!await authorization.CurrentUserHasAsync(UserAccessPermissions.ScopesRead, request.Id, cancellationToken))
        {
            return Error.Forbidden();
        }

        Versioned<ScopeQueryResponse>? scope = await repository.GetScopeById(request.Id, cancellationToken);

        return scope is null ? Error.NotFound() : scope;
    }
}
