using Sergin.SharedKernel.Application.Commands;

namespace Sergin.UserAccess.Application.Contracts.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid Id) : ICommand<DeactivateUserCommandResponse>;
