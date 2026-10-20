using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts.Scopes;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Create;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Scopes.Commands.Create;

internal sealed class CreateScopeCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IScopeRepository scopes,
    IScopeAuthorization authorization,
    IAuthorizationCacheInvalidator cache) : ICommandHandler<CreateScopeCommand, CreateScopeCommandResponse>
{
    public async Task<ErrorOr<CreateScopeCommandResponse>> Handle(CreateScopeCommand request, CancellationToken cancellationToken)
    {
        if (!await authorization.CurrentUserHasAsync(ScopePermissions.Manage, request.ParentId, cancellationToken))
        {
            return Error.Forbidden();
        }

        Scope? parent = await scopes.GetAsync(new ScopeId(request.ParentId), cancellationToken);

        if (parent is null)
        {
            return Error.NotFound();
        }

        var scope = Scope.Create(request.Name, parent);
        scopes.Insert(scope);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.Invalidate();

        return new CreateScopeCommandResponse(scope.Id.Value);
    }
}
