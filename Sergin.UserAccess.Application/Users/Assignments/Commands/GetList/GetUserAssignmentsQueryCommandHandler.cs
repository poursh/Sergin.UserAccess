using Sergin.SharedKernel.Application.Commands.Queries;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;

namespace Sergin.UserAccess.Application.Users.Assignments.Commands.GetList;

internal sealed class GetUserAssignmentsQueryCommandHandler(IUserAssignmentQueryRepository repository, UserReachGuard reach)
    : IQueryHandler<GetUserAssignmentsQueryCommand, IReadOnlyCollection<UserAssignmentItem>>
{
    public async Task<ErrorOr<IReadOnlyCollection<UserAssignmentItem>>> Handle(
        GetUserAssignmentsQueryCommand request, CancellationToken cancellationToken)
    {
        // A regional reader does not see the grants of someone granted outside their region.
        if (!await reach.CoversAsync(request.UserId, UserAccessPermissions.UsersRead, cancellationToken))
        {
            return Error.Forbidden();
        }

        return (await repository.GetUserAssignmentsAsync(request.UserId, cancellationToken)).ToErrorOr();
    }
}
