using Sergin.SharedKernel.Application.Commands;

namespace Sergin.UserAccess.Application.Contracts.Scopes.Commands.Delete;

/// <summary>
/// Deletes a leaf scope. Refused while assignments are live at it or roles are owned by it, unless
/// <paramref name="Cascade"/> is set, which revokes those assignments and deletes those roles (BR-5).
/// </summary>
public sealed record DeleteScopeCommand(Guid Id, bool Cascade) : ICommand<Deleted>;
