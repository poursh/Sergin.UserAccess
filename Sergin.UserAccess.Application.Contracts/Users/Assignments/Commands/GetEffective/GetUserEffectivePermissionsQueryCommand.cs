using Sergin.SharedKernel.Application.Commands.Queries;

namespace Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetEffective;

/// <summary>FR-11: what the user can do right now, per scope an active assignment reaches from.</summary>
public sealed record GetUserEffectivePermissionsQueryCommand(Guid UserId) : IQuery<IReadOnlyCollection<EffectivePermissionItem>>;
