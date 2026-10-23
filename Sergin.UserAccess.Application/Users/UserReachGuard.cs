using Sergin.SharedKernel.Application.Securities.Authorization;
using Sergin.SharedKernel.Application.Times;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;
using Sergin.UserAccess.Application.Users.Assignments;

namespace Sergin.UserAccess.Application.Users;

/// <summary>
/// Whether the caller may act on one user. A user belongs to no scope, so the pipeline only asks that the caller
/// holds the permission somewhere; this adds that every live assignment the user holds sits inside the caller's
/// reach for that permission. A regional administrator therefore acts on the users of their region, and never on
/// someone granted above or beside it, such as the root administrator. A user with no live assignment is in
/// everyone's reach. A revoked or expired assignment grants nothing and is not counted; one not yet started is,
/// since it will grant. An assignment on a deleted scope grants nothing and is skipped.
/// </summary>
internal sealed class UserReachGuard(
    IScopeAuthorization authorization,
    IUserAssignmentQueryRepository assignments,
    IScopePathResolver paths,
    IDateTimeProvider clock)
{
    public async Task<bool> CoversAsync(Guid userId, PermissionDefinition permission, CancellationToken cancellationToken)
    {
        ScopeReach reach = await authorization.CurrentUserReachAsync(permission, cancellationToken);

        if (reach.Unrestricted)
        {
            return true;
        }

        DateTime now = clock.UtcNow;

        foreach (UserAssignmentItem assignment in await assignments.GetUserAssignmentsAsync(userId, cancellationToken))
        {
            if (assignment.RevokedAtUtc is not null || assignment.ValidUntilUtc <= now)
            {
                continue;
            }

            if (await paths.GetPathAsync(assignment.ScopeId, cancellationToken) is not { } path)
            {
                continue;
            }

            ScopeReachEntry target = new(assignment.ScopeId, path, assignment.IncludeSubtree);

            if (!reach.Entries.Any(entry => entry.Covers(target)))
            {
                return false;
            }
        }

        return true;
    }
}
