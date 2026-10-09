using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.SetInheritable;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.SetInheritable;

// No RequirePermissions: the permission is checked against the target scope in the handler, since a regional
// administrator holds it there only and the global claims would refuse them.
internal sealed class SetRoleInheritableCommandConfiguration : ICommandConfiguration<SetRoleInheritableCommand>
{
    public void Configure(CommandConfigurationBuilder<SetRoleInheritableCommand> builder) => builder.RequireExpectedVersion();
}
