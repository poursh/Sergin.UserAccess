using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.RevokePermission;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.RevokePermission;

// No RequirePermissions: the permission is checked against the target scope in the handler, which is the
// record's own scope rather than the scope the caller is working in.
internal sealed class RevokeRolePermissionCommandConfiguration : ICommandConfiguration<RevokeRolePermissionCommand>
{
    public void Configure(CommandConfigurationBuilder<RevokeRolePermissionCommand> builder) => builder.RequireExpectedVersion();
}
