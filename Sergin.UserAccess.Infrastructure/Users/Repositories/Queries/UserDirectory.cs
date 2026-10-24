using System.Data.Common;
using Sergin.SharedKernel.Application.Securities.Users;
using Sergin.SharedKernel.Infrastracture.Data;

namespace Sergin.UserAccess.Infrastructure.Users.Repositories.Queries;

/// <summary>
/// The user store's answer to <see cref="IUserDirectory"/>: first and last name where either is set, otherwise the
/// user name. A deactivated user keeps their name — an audit stamp still names who wrote the row.
/// </summary>
internal sealed class UserDirectory(IDbConnectionFactory connectionFactory) : IUserDirectory
{
    public async Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        const string query =
            """
            SELECT id,
                   COALESCE(NULLIF(btrim(concat_ws(' ', first_name, last_name)), ''), user_name) AS displayName
            FROM ua.users
            WHERE id = ANY(@Ids);
            """;

        IEnumerable<DisplayNameRow> rows =
            await connection.QueryAsync<DisplayNameRow>(query, new { Ids = userIds.ToArray() });

        return rows.ToDictionary(row => row.Id, row => row.DisplayName);
    }

    private sealed record DisplayNameRow(Guid Id, string DisplayName);
}
