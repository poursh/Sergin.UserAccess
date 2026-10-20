using Sergin.SharedKernel.Application.Aggregates;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Application.Configurations.Roles;

internal sealed class RoleAggregateFeatureConfiguration : IAggregateFeatureConfiguration<Role>
{
    public void Configure(AggregateFeatureBuilder<Role> builder) => builder.Audited().SoftDeletable().Versioned()
            .Permissions(RolePermissions.Read, RolePermissions.Manage);
}
