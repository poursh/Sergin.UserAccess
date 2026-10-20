using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Configurations.Users.Assignments.Commands.GetList;

internal sealed class GetUserAssignmentsQueryCommandConfiguration : ICommandConfiguration<GetUserAssignmentsQueryCommand, User>
{
    public void Configure(CommandConfigurationBuilder<GetUserAssignmentsQueryCommand> builder) =>
        builder.RequirePermissions(UserPermissions.Read);
}
