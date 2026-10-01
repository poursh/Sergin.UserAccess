using Sergin.SharedKernel.Application.Commands.Configuration;

namespace Sergin.UserAccess.Application.Contracts.Users.Commands.GetOne;

internal sealed class GetUserByIdQueryCommandConfiguration : ICommandConfiguration<GetUserByIdQueryCommand>
{
    public void Configure(CommandConfigurationBuilder<GetUserByIdQueryCommand> builder) =>
        builder.RequirePermissions("permission.ua.users.read");
}
