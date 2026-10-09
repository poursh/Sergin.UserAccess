using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sergin.UserAccess.Domain.Scopes;

namespace Sergin.UserAccess.Infrastructure.Data.Scopes.Converters;

internal sealed class ScopeNameConverter : ValueConverter<ScopeName, string>
{
    private static readonly ConverterMappingHints defaultHints = new();

    public ScopeNameConverter() : this(null)
    {
    }

    public ScopeNameConverter(ConverterMappingHints? mappingHints)
        : base(
                convertToProviderExpression: x => x.Value,
                convertFromProviderExpression: x => new ScopeName(x),
                mappingHints: defaultHints.With(mappingHints))
    {
    }
}
