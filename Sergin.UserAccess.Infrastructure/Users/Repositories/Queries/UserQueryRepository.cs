using System.Data.Common;
using Sergin.UserAccess.Application.Users;
using Sergin.UserAccess.Application.Users.Commands.GetList;
using Sergin.SharedKernel.Application;
using Sergin.SharedKernel.Application.Commands.Queries;
using Sergin.SharedKernel.Infrastracture.Data;
using Sergin.UserAccess.Application.Users.Commands.GetOne;
using Sergin.SharedKernel.Application.Securities;
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
            FROM ua.user_roles ur
            JOIN ua.role_permissions rp ON rp.role_id = ur.role_id
            WHERE ur.user_id = @UserId;
            """;

        IEnumerable<string> codes = await connection.QueryAsync<string>(queries, new { UserId = userId.Value });

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
