using Sergin.SharedKernel.Application.Commands.Queries;

namespace Sergin.UserAccess.Application.Contracts.Users.Commands.GetOne;

public sealed record GetUserByIdQueryCommand(Guid Id) : IQuery<UserQueryResponse>;
