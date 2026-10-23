using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Users.Commands.GetOne;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Configurations.Users.Commands.GetOne;

internal sealed class GetUserByIdQueryCommandConfiguration : ICommandConfiguration<GetUserByIdQueryCommand, User>
{
    public void Configure(CommandConfigurationBuilder<GetUserByIdQueryCommand> builder) =>
        builder.RequirePermissions(UserPermissions.Read);
}
