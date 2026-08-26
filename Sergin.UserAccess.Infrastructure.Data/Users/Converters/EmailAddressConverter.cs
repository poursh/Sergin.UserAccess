using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Infrastructure.Data.Users.Converters;

internal sealed class EmailAddressConverter : ValueConverter<EmailAddress?, string?>
{
    private static readonly ConverterMappingHints defaultHints = new();

    public EmailAddressConverter() : this(null)
    {
    }

    public EmailAddressConverter(ConverterMappingHints? mappingHints)
        : base(
                convertToProviderExpression: x => x == null ? null : x.Value,
                convertFromProviderExpression: x => x == null ? null : new EmailAddress(x),
                mappingHints: defaultHints.With(mappingHints))
    {
    }
}
