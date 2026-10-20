using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Users;
using Sergin.UserAccess.Application.Contracts.Users.Commands.GetList;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Configurations.Users.Commands.GetList;

internal sealed class GetUserListQueryCommandConfiguration : ICommandConfiguration<GetUserListQueryCommand, User>
{
    public void Configure(CommandConfigurationBuilder<GetUserListQueryCommand> builder) =>
        builder.RequirePermissions(UserPermissions.Read);
}
