using Sergin.SharedKernel.Application.Commands.Configuration;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;

namespace Sergin.UserAccess.Application.Configurations.Users.Assignments.Commands.GetList;

internal sealed class GetUserAssignmentsQueryCommandConfiguration : ICommandConfiguration<GetUserAssignmentsQueryCommand>
{
    public void Configure(CommandConfigurationBuilder<GetUserAssignmentsQueryCommand> builder) =>
        builder.RequirePermissions(UserAccessPermissions.UsersRead);
}
