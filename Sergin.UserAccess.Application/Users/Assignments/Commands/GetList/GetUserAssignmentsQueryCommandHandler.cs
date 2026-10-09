using Sergin.SharedKernel.Application.Commands.Queries;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;

namespace Sergin.UserAccess.Application.Users.Assignments.Commands.GetList;

internal sealed class GetUserAssignmentsQueryCommandHandler(IUserAssignmentQueryRepository repository)
    : IQueryHandler<GetUserAssignmentsQueryCommand, IReadOnlyCollection<UserAssignmentItem>>
{
    public async Task<ErrorOr<IReadOnlyCollection<UserAssignmentItem>>> Handle(
        GetUserAssignmentsQueryCommand request, CancellationToken cancellationToken) =>
        (await repository.GetUserAssignmentsAsync(request.UserId, cancellationToken)).ToErrorOr();
}
