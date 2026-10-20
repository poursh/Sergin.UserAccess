using Sergin.SharedKernel.Application.Aggregates;
using Sergin.UserAccess.Application.Contracts.Users;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Application.Configurations.Users;

internal sealed class UserAggregateFeatureConfiguration : IAggregateFeatureConfiguration<User>
{
    public void Configure(AggregateFeatureBuilder<User> builder) =>
        builder.Permissions(UserPermissions.Read, UserPermissions.Manage, UserPermissions.AssignmentsManage);
}
