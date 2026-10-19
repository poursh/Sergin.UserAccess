using Sergin.SharedKernel.Application.Commands;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Contracts.Scopes.Commands.Rename;

public sealed record RenameScopeCommand(Guid Id, ScopeName Name) : ICommand<RenameScopeCommandResponse>;
