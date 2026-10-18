using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.SetInheritable;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Configurations.Roles.Commands.SetInheritable;

// No RequirePermissions: the permission is checked against the target scope in the handler, which is the
// record's own scope rather than the scope the caller is working in.
internal sealed class SetRoleInheritableCommandConfiguration : ICommandConfiguration<SetRoleInheritableCommand, Role>
{
    public void Configure(CommandConfigurationBuilder<SetRoleInheritableCommand> builder) => builder.RequireExpectedVersion();
}
