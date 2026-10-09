using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.RevokePermission;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.RevokePermission;

// No RequirePermissions: the permission is checked against the target scope in the handler, since a regional
// administrator holds it there only and the global claims would refuse them.
internal sealed class RevokeRolePermissionCommandConfiguration : ICommandConfiguration<RevokeRolePermissionCommand>
{
    public void Configure(CommandConfigurationBuilder<RevokeRolePermissionCommand> builder) => builder.RequireExpectedVersion();
}
