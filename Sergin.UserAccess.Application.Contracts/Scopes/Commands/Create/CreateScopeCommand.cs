using Sergin.SharedKernel.Application.Commands;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Contracts.Scopes.Commands.Create;

public sealed record CreateScopeCommand(Guid ParentId, ScopeName Name) : ICommand<CreateScopeCommandResponse>;
