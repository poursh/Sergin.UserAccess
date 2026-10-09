using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Users.Commands.Create;

namespace Sergin.UserAccess.Application.Configurations.Users.Commands.Create;

internal sealed class CreateUserCommandConfiguration : ICommandConfiguration<CreateUserCommand>
{
    public void Configure(CommandConfigurationBuilder<CreateUserCommand> builder) =>
        builder.RequirePermissions(UserAccessPermissions.UsersManage);
}
