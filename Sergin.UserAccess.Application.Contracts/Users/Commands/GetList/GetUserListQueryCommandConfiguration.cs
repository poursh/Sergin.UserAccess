using Sergin.SharedKernel.Application.Commands.Configuration;

namespace Sergin.UserAccess.Application.Contracts.Users.Commands.GetList;

internal sealed class GetUserListQueryCommandConfiguration : ICommandConfiguration<GetUserListQueryCommand>
{
    public void Configure(CommandConfigurationBuilder<GetUserListQueryCommand> builder) =>
        builder.RequirePermissions("permission.ua.users.read");
}
