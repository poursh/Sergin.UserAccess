using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Delete;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.Delete;

// No RequirePermissions: the permission is checked against the target scope in the handler, which is the
// record's own scope rather than the scope the caller is working in.
internal sealed class DeleteRoleCommandConfiguration : ICommandConfiguration<DeleteRoleCommand, Role>
{
    public void Configure(CommandConfigurationBuilder<DeleteRoleCommand> builder) => builder.RequireExpectedVersion();
}
