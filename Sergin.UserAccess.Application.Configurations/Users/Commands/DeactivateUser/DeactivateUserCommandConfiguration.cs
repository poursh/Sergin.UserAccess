using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Users.Commands.DeactivateUser;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Configurations.Users.Commands.DeactivateUser;

internal sealed class DeactivateUserCommandConfiguration : ICommandConfiguration<DeactivateUserCommand, User>
{
    public void Configure(CommandConfigurationBuilder<DeactivateUserCommand> builder) =>
        builder.RequirePermissions(UserAccessPermissions.UsersManage);
}
