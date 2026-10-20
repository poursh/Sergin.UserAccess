using Sergin.UserAccess.Domain.Users;
using Sergin.SharedKernel.Application.Commands;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Configurations.Users;

namespace Sergin.UserAccess.Application.Users.Commands.DeactivateUser;

internal sealed class DeactivateUserCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IUserRepository repository,
    IAuthorizationCacheInvalidator cache,
    UserReachGuard reach) : ICommandHandler<DeactivateUserCommand, DeactivateUserCommandResponse>
{
    public async Task<ErrorOr<DeactivateUserCommandResponse>> Handle(
        DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await repository.GetAsync(new UserInternalId(request.Id), cancellationToken);

        if (user is null)
        {
            return Error.NotFound();
        }

        // A regional administrator may not deactivate someone granted outside their region, the root administrator above all.
        if (!await reach.CoversAsync(user.Id.Value, UserPermissions.Manage, cancellationToken))
        {
            return Error.Forbidden();
        }

        user.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // FR-13: every scoped check for this user is denied from the next request on.
        cache.Invalidate();

        return new DeactivateUserCommandResponse(user.Id.Value);
    }
}
