using Sergin.SharedKernel.Application.Commands.Queries;

namespace Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;

/// <summary>Every assignment the user has ever held, live first, revoked ones kept for history.</summary>
public sealed record GetUserAssignmentsQueryCommand(Guid UserId) : IQuery<IReadOnlyCollection<UserAssignmentItem>>;
