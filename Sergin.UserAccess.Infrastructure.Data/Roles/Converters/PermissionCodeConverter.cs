using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sergin.UserAccess.Domain.Roles;

namespace Sergin.UserAccess.Infrastructure.Data.Roles.Converters;

internal sealed class PermissionCodeConverter : ValueConverter<PermissionCode, string>
{
    private static readonly ConverterMappingHints defaultHints = new();

    public PermissionCodeConverter() : this(null)
    {
    }

    public PermissionCodeConverter(ConverterMappingHints? mappingHints)
        : base(
                convertToProviderExpression: x => x.Value,
                convertFromProviderExpression: x => new PermissionCode(x),
                mappingHints: defaultHints.With(mappingHints))
    {
    }
}
