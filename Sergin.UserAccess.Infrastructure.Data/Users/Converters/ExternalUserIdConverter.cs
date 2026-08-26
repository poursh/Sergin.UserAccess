using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Infrastructure.Data.Users.Converters;

internal sealed class ExternalUserIdConverter : ValueConverter<ExternalUserId?, string?>
{
    private static readonly ConverterMappingHints defaultHints = new();

    public ExternalUserIdConverter() : this(null)
    {
    }

    public ExternalUserIdConverter(ConverterMappingHints? mappingHints)
        : base(
                convertToProviderExpression: x => x == null ? null : x.Value,
                convertFromProviderExpression: x => x == null ? null : new ExternalUserId(x),
                mappingHints: defaultHints.With(mappingHints))
    {
    }
}
