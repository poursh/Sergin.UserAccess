using System.Data.Common;
using Sergin.SharedKernel.Infrastracture.Data;
using Sergin.UserAccess.Application.Authorization;

namespace Sergin.UserAccess.Infrastructure.Authorization;

internal sealed class EffectiveGrantQueryRepository(IDbConnectionFactory connectionFactory) : IEffectiveGrantQueryRepository
{
    public async Task<IReadOnlyCollection<EffectiveGrantRow>> GetGrantsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        // One read over the partial index on live assignments. The start of the validity window is left to the
        // caller, which evaluates it against its own clock on every check, so a cached grant that becomes valid
        // later starts applying without a reload; an assignment already expired is never loaded.
        string query =
            """
            SELECT a.id AS assignmentId,
                   a.scope_id AS scopeId,
                   s.path::text AS scopePath,
                   a.include_subtree AS includeSubtree,
                   a.valid_from_utc AS validFromUtc,
                   a.valid_until_utc AS validUntilUtc,
                   rp.permission AS permission
            FROM ua.role_assignments a
            JOIN ua.users u ON u.id = a.user_id AND u.is_active
            JOIN ua.roles r ON r.id = a.role_id AND r.deleted_at_utc IS NULL
            JOIN ua.scopes s ON s.id = a.scope_id AND s.deleted_at_utc IS NULL
            JOIN ua.role_permissions rp ON rp.role_id = a.role_id
            WHERE a.user_id = @UserId
              AND a.revoked_at_utc IS NULL
              AND (a.valid_until_utc IS NULL OR a.valid_until_utc > now());
            """;

        return [.. await connection.QueryAsync<EffectiveGrantRow>(query, new { UserId = userId })];
    }

    public async Task<string?> GetScopePathAsync(Guid scopeId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT path::text
            FROM ua.scopes
            WHERE id = @ScopeId AND deleted_at_utc IS NULL;
            """;

        return await connection.QuerySingleOrDefaultAsync<string>(query, new { ScopeId = scopeId });
    }
}
