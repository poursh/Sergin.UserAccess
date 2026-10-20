using Sergin.SharedKernel.Application.Aggregates;
using Sergin.UserAccess.Application.Contracts.Scopes;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Configurations.Scopes;

internal sealed class ScopeAggregateFeatureConfiguration : IAggregateFeatureConfiguration<Scope>
{
    public void Configure(AggregateFeatureBuilder<Scope> builder) => builder.Audited().SoftDeletable().Versioned()
            .Permissions(ScopePermissions.Read, ScopePermissions.Manage);
}
