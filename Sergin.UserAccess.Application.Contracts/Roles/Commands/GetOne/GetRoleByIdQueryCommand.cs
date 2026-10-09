using Sergin.SharedKernel.Application.Commands.Queries;

namespace Sergin.UserAccess.Application.Contracts.Roles.Commands.GetOne;

public sealed record GetRoleByIdQueryCommand(Guid Id) : IQuery<RoleQueryResponse>;
