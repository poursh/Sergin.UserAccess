using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Users.Commands.DeactivateUser;

namespace Sergin.UserAccess.Application.Configurations.Users.Commands.DeactivateUser;

internal sealed class DeactivateUserCommandConfiguration : ICommandConfiguration<DeactivateUserCommand>
{
    public void Configure(CommandConfigurationBuilder<DeactivateUserCommand> builder) =>
        builder.RequirePermissions(UserAccessPermissions.UsersManage).AtRoot();
}
