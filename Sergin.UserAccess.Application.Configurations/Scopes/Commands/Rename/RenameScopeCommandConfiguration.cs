using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Rename;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Configurations.Scopes.Commands.Rename;

// No RequirePermissions: the permission is checked against the target scope in the handler, which is the
// record's own scope rather than the scope the caller is working in.
internal sealed class RenameScopeCommandConfiguration : ICommandConfiguration<RenameScopeCommand, Scope>
{
    public void Configure(CommandConfigurationBuilder<RenameScopeCommand> builder) => builder.RequireExpectedVersion();
}
