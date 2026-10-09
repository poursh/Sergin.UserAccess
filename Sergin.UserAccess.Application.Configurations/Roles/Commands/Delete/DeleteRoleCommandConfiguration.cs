using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.Delete;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.Delete;

// No RequirePermissions: the permission is checked against the target scope in the handler, since a regional
// administrator holds it there only and the global claims would refuse them.
internal sealed class DeleteRoleCommandConfiguration : ICommandConfiguration<DeleteRoleCommand>
{
    public void Configure(CommandConfigurationBuilder<DeleteRoleCommand> builder) => builder.RequireExpectedVersion();
}
