using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.RevokePermission;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.RevokePermission;

// No RequirePermissions: the permission is checked against the target scope in the handler, which is the
// record's own scope rather than the scope the caller is working in.
internal sealed class RevokeRolePermissionCommandConfiguration : ICommandConfiguration<RevokeRolePermissionCommand, Role>
{
    public void Configure(CommandConfigurationBuilder<RevokeRolePermissionCommand> builder) => builder.RequireExpectedVersion();
}
