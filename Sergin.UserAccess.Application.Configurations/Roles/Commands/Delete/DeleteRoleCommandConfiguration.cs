using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Delete;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.Delete;

// No RequirePermissions: the permission is checked against the target scope in the handler, which is the
// record's own scope rather than the scope the caller is working in.
internal sealed class DeleteRoleCommandConfiguration : ICommandConfiguration<DeleteRoleCommand>
{
    public void Configure(CommandConfigurationBuilder<DeleteRoleCommand> builder) => builder.RequireExpectedVersion();
}
