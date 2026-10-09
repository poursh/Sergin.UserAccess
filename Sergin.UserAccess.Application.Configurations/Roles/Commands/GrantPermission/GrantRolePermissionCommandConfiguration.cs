using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GrantPermission;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.GrantPermission;

// No RequirePermissions: the permission is checked against the target scope in the handler, since a regional
// administrator holds it there only and the global claims would refuse them.
internal sealed class GrantRolePermissionCommandConfiguration : ICommandConfiguration<GrantRolePermissionCommand>
{
    public void Configure(CommandConfigurationBuilder<GrantRolePermissionCommand> builder) => builder.RequireExpectedVersion();
}
