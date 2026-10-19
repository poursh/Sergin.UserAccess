using Sergin.SharedKernel.Application.Commands.Queries;

namespace Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetOne;

public sealed record GetScopeByIdQueryCommand(Guid Id) : IQuery<ScopeQueryResponse>;
