using Sergin.SharedKernel.Application.Commands;
using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.UserAccess.Application.Authorization;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.Revoke;
using Sergin.SharedKernel.Application.Securities.Users;
using Sergin.SharedKernel.Application.Times;
using Sergin.UserAccess.Domain.Users;
using Sergin.UserAccess.Domain.Users.Assignments;
using Sergin.UserAccess.Application.Configurations.Users;

namespace Sergin.UserAccess.Application.Users.Assignments.Commands.Revoke;

internal sealed class RevokeAssignmentCommandHandler(
    IUserAccessUnitOfWork unitOfWork,
    IUserRepository users,
    IScopeAuthorization authorization,
    IUserContext userContext,
    IDateTimeProvider clock,
    IAuthorizationCacheInvalidator cache) : ICommandHandler<RevokeAssignmentCommand, RevokeAssignmentCommandResponse>
{
    public async Task<ErrorOr<RevokeAssignmentCommandResponse>> Handle(RevokeAssignmentCommand request, CancellationToken cancellationToken)
    {
        User? user = await users.GetWithAssignmentsAsync(new UserInternalId(request.UserId), cancellationToken);
        var assignmentId = new RoleAssignmentId(request.AssignmentId);
        RoleAssignment? assignment = user?.Assignments.FirstOrDefault(a => a.Id == assignmentId);

        if (user is null || assignment is null)
        {
            return Error.NotFound();
        }

        if (!await authorization.CurrentUserHasAsync(UserPermissions.AssignmentsManage, assignment.ScopeId.Value, cancellationToken))
        {
            return Error.Forbidden();
        }

        ErrorOr<Success> revoked = user.RevokeAssignment(assignmentId, clock.UtcNow, userContext.Id.Value);

        if (revoked.IsError)
        {
            return revoked.Errors;
        }

        // FR-12: effective immediately — the cache is dropped as soon as the revocation is committed.
        await unitOfWork.SaveChangesAsync(cancellationToken);
        cache.Invalidate();

        return new RevokeAssignmentCommandResponse(request.AssignmentId);
    }
}
