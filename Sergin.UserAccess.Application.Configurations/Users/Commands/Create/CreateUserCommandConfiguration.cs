using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Users;
using Sergin.UserAccess.Application.Contracts.Users.Commands.Create;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Configurations.Users.Commands.Create;

internal sealed class CreateUserCommandConfiguration : ICommandConfiguration<CreateUserCommand, User>
{
    public void Configure(CommandConfigurationBuilder<CreateUserCommand> builder) =>
        builder.RequirePermissions(UserPermissions.Manage);
}
