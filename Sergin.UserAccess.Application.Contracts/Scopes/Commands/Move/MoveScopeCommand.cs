using Sergin.SharedKernel.Application.Commands;

namespace Sergin.UserAccess.Application.Contracts.Scopes.Commands.Move;

public sealed record MoveScopeCommand(Guid Id, Guid NewParentId) : ICommand<MoveScopeCommandResponse>;
