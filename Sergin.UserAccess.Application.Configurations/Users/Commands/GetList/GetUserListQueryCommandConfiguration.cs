using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Users.Commands.GetList;

namespace Sergin.UserAccess.Application.Configurations.Users.Commands.GetList;

internal sealed class GetUserListQueryCommandConfiguration : ICommandConfiguration<GetUserListQueryCommand>
{
    public void Configure(CommandConfigurationBuilder<GetUserListQueryCommand> builder) =>
        builder.RequirePermissions("permission.ua.users.read");
}
