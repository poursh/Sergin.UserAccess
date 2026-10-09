using System.Data.Common;
using Sergin.UserAccess.Application.Users;
using Sergin.UserAccess.Application.Contracts.Users.Commands.GetList;
using Sergin.SharedKernel.Application;
using Sergin.SharedKernel.Application.Commands.Queries;
using Sergin.SharedKernel.Infrastracture.Data;
using Sergin.UserAccess.Application.Contracts.Users.Commands.GetOne;
using Sergin.SharedKernel.Domain.Securities;
using Sergin.UserAccess.Domain.Scopes;
using Sergin.UserAccess.Domain.Users;

namespace Sergin.UserAccess.Infrastructure.Users.Repositories.Queries;

internal sealed class UserQueryRepository(
    IDbConnectionFactory connectionFactory) : IUserAllQueryRepository
{
    public async Task<UserQueryResponse?> GetUserById(
        UserInternalId Id, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string queries =
           """
            SELECT id, user_name AS userName
            FROM ua.users
            WHERE id = @Id;
            """;

        return await connection.QuerySingleOrDefaultAsync<UserQueryResponse>(
            queries, new { Id = Id.Value });
    }

    public async Task<IReadOnlyCollection<Permission>> GetPermissions(
        UserInternalId userId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string queries =
            """
            SELECT DISTINCT rp.permission
            FROM ua.role_assignments a
            JOIN ua.users u ON u.id = a.user_id AND u.is_active
            JOIN ua.roles r ON r.id = a.role_id AND r.deleted_at_utc IS NULL
            JOIN ua.role_permissions rp ON rp.role_id = a.role_id
            WHERE a.user_id = @UserId
              AND a.scope_id = @RootId
              AND a.include_subtree
              AND a.revoked_at_utc IS NULL
              AND a.valid_until_utc IS NULL
              AND (a.valid_from_utc IS NULL OR a.valid_from_utc <= now());
            """;

        // Global permissions only, those effective on every scope: a root assignment over the whole tree. They
        // become the sign-in claims, which outlive the request, so a time-bound assignment is left out: it would
        // stay in the cookie past its end. Scoped and time-bound grants are answered live by the scoped check.
        // An inactive user gets none (FR-13).
        IEnumerable<string> codes = await connection.QueryAsync<string>(
            queries, new { UserId = userId.Value, RootId = Scope.RootId.Value });

        // A code that no longer parses is a permission the format outgrew; dropping it narrows the
        // user's rights, which is the safe direction, rather than failing their sign-in outright.
        return [.. codes.Select(code => Permission.TryCreate(code, out Permission? permission) ? permission : null)
            .OfType<Permission>()];
    }

    public async Task<ListQueryResponse<GetUserListItem>> GetListAsync(
        ListQuery query, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string queries =
            """
            SELECT count(*) FROM ua.users;

            SELECT id, user_name AS userName
            FROM ua.users
            ORDER BY id
            LIMIT @PageSize OFFSET @Offset;
            """;

        GridReader res = await connection.QueryMultipleAsync(
            queries, new { PageSize = query.Paggination.Size.Value, Offset = query.Paggination.Skip });

        int count = await res.ReadSingleAsync<int>();
        IReadOnlyCollection<GetUserListItem> list = [.. await res.ReadAsync<GetUserListItem>()];

        return new ListQueryResponse<GetUserListItem>(list, count);
    }
}
