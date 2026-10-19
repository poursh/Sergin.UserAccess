using Sergin.SharedKernel.Application.Commands;

namespace Sergin.UserAccess.Application.Contracts.Roles.Commands.Delete;

/// <summary>Deletes a role. Refused while it has live assignments, unless <paramref name="Cascade"/> revokes them (BR-5).</summary>
public sealed record DeleteRoleCommand(Guid Id, bool Cascade) : ICommand<Deleted>;
