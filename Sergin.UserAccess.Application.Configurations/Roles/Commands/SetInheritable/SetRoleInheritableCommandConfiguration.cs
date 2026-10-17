using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.SetInheritable;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.SetInheritable;

// No RequirePermissions: the permission is checked against the target scope in the handler, which is the
// record's own scope rather than the scope the caller is working in.
internal sealed class SetRoleInheritableCommandConfiguration : ICommandConfiguration<SetRoleInheritableCommand>
{
    public void Configure(CommandConfigurationBuilder<SetRoleInheritableCommand> builder) => builder.RequireExpectedVersion();
}
