using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetEffective;

namespace Sergin.UserAccess.Application.Configurations.Users.Assignments.Commands.GetEffective;

internal sealed class GetUserEffectivePermissionsQueryCommandConfiguration : ICommandConfiguration<GetUserEffectivePermissionsQueryCommand>
{
    public void Configure(CommandConfigurationBuilder<GetUserEffectivePermissionsQueryCommand> builder) =>
        builder.RequirePermissions(UserAccessPermissions.UsersRead);
}
