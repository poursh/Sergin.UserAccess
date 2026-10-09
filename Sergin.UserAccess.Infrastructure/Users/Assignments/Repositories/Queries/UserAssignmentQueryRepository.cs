using System.Data.Common;
using Sergin.SharedKernel.Infrastracture.Data;
using Sergin.UserAccess.Application.Contracts.Users.Assignments.Commands.GetList;
using Sergin.UserAccess.Application.Users.Assignments;

namespace Sergin.UserAccess.Infrastructure.Users.Assignments.Repositories.Queries;

internal sealed class UserAssignmentQueryRepository(IDbConnectionFactory connectionFactory) : IUserAssignmentQueryRepository
{
    public async Task<IReadOnlyCollection<UserAssignmentItem>> GetUserAssignmentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        // History included: a revoked assignment, or one whose role or scope was deleted, still shows what it was.
        string query =
            """
            SELECT a.id, a.role_id AS roleId, r.name AS roleName, a.scope_id AS scopeId, s.name AS scopeName,
                   a.include_subtree AS includeSubtree, a.valid_from_utc AS validFromUtc, a.valid_until_utc AS validUntilUtc,
                   a.revoked_at_utc AS revokedAtUtc
            FROM ua.role_assignments a
            JOIN ua.roles r ON r.id = a.role_id
            JOIN ua.scopes s ON s.id = a.scope_id
            WHERE a.user_id = @UserId
            ORDER BY a.revoked_at_utc IS NOT NULL, s.name, r.name;
            """;

        return [.. await connection.QueryAsync<UserAssignmentItem>(query, new { UserId = userId })];
    }
}
