using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Users.Commands.GetOne;

namespace Sergin.UserAccess.Application.Configurations.Users.Commands.GetOne;

internal sealed class GetUserByIdQueryCommandConfiguration : ICommandConfiguration<GetUserByIdQueryCommand>
{
    public void Configure(CommandConfigurationBuilder<GetUserByIdQueryCommand> builder) =>
        builder.RequirePermissions("permission.ua.users.read");
}
