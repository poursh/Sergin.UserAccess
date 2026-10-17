using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GrantPermission;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.GrantPermission;

// No RequirePermissions: the permission is checked against the target scope in the handler, which is the
// record's own scope rather than the scope the caller is working in.
internal sealed class GrantRolePermissionCommandConfiguration : ICommandConfiguration<GrantRolePermissionCommand>
{
    public void Configure(CommandConfigurationBuilder<GrantRolePermissionCommand> builder) => builder.RequireExpectedVersion();
}
