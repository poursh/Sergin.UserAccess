using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;

namespace Sergin.UserAccess.Application.Users.Assignments;

public interface IUserAssignmentQueryRepository
{
    Task<IReadOnlyCollection<UserAssignmentItem>> GetUserAssignmentsAsync(Guid userId, CancellationToken cancellationToken = default);
}
