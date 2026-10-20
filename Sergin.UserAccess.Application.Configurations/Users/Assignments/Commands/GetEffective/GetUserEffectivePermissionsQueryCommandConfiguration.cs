using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetEffective;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Configurations.Users.Assignments.Commands.GetEffective;

internal sealed class GetUserEffectivePermissionsQueryCommandConfiguration : ICommandConfiguration<GetUserEffectivePermissionsQueryCommand, User>
{
    public void Configure(CommandConfigurationBuilder<GetUserEffectivePermissionsQueryCommand> builder) =>
        builder.RequirePermissions(UserPermissions.Read);
}
