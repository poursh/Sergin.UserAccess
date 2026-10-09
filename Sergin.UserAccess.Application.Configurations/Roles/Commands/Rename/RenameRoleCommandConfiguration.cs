using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Rename;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.Rename;

// No RequirePermissions: the permission is checked against the target scope in the handler, since a regional
// administrator holds it there only and the global claims would refuse them.
internal sealed class RenameRoleCommandConfiguration : ICommandConfiguration<RenameRoleCommand>
{
    public void Configure(CommandConfigurationBuilder<RenameRoleCommand> builder) => builder.RequireExpectedVersion();
}
