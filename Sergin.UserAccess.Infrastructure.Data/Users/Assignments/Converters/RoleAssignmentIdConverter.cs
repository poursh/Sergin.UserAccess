using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sergin.UserAccess.Domain.Users.Assignments;

namespace Sergin.UserAccess.Infrastructure.Data.Users.Assignments.Converters;

internal sealed class RoleAssignmentIdConverter : ValueConverter<RoleAssignmentId, Guid>
{
    private static readonly ConverterMappingHints defaultHints = new();

    public RoleAssignmentIdConverter() : this(null)
    {
    }

    public RoleAssignmentIdConverter(ConverterMappingHints? mappingHints)
        : base(
                convertToProviderExpression: x => x.Value,
                convertFromProviderExpression: x => new RoleAssignmentId(x),
                mappingHints: defaultHints.With(mappingHints))
    {
    }
}
