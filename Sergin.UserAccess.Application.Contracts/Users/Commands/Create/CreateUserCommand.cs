using Sergin.UserAccess.Domain.Users;
using Sergin.SharedKernel.Application.Commands;

namespace Sergin.UserAccess.Application.Contracts.Users.Commands.Create;

public sealed record CreateUserCommand(UserName UserName) : ICommand<CreateUserCommandResponse>;
