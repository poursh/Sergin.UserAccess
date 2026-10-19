using Sergin.SharedKernel.Application.Aggregates;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Application.Configurations.Scopes;

internal sealed class ScopeAggregateFeatureConfiguration : IAggregateFeatureConfiguration<Scope>
{
    public void Configure(AggregateFeatureBuilder<Scope> builder) => builder.Audited().SoftDeletable().Versioned();
}
