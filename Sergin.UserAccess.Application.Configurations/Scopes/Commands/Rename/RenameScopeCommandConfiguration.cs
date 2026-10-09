using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.Rename;

namespace Sergin.UserAccess.Application.Configurations.Scopes.Commands.Rename;

// No RequirePermissions: the permission is checked against the target scope in the handler, since a regional
// administrator holds it there only and the global claims would refuse them.
internal sealed class RenameScopeCommandConfiguration : ICommandConfiguration<RenameScopeCommand>
{
    public void Configure(CommandConfigurationBuilder<RenameScopeCommand> builder) => builder.RequireExpectedVersion();
}
