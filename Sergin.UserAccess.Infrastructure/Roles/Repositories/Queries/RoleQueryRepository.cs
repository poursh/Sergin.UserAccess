using System.Data.Common;
using Sergin.SharedKernel.Application.Concurrency;
using Sergin.SharedKernel.Domain;
using Sergin.SharedKernel.Infrastracture.Data;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GetAssignable;
using Sergin.UserAccess.Application.Contracts.Roles.Commands.GetOne;
using Sergin.UserAccess.Application.Roles;

namespace Sergin.UserAccess.Infrastructure.Roles.Repositories.Queries;

internal sealed class RoleQueryRepository(IDbConnectionFactory connectionFactory) : IRoleQueryRepository
{
    public async Task<Versioned<RoleQueryResponse>?> GetRoleById(Guid id, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT r.id, r.owner_scope_id AS ownerScopeId, o.name AS ownerScopeName, r.name, r.is_inheritable AS isInheritable,
                   r.row_version AS rowVersion
            FROM ua.roles r
            JOIN ua.scopes o ON o.id = r.owner_scope_id
            WHERE r.id = @Id AND r.deleted_at_utc IS NULL;

            SELECT permission FROM ua.role_permissions WHERE role_id = @Id ORDER BY permission;
            """;

        GridReader res = await connection.QueryMultipleAsync(query, new { Id = id });

        RoleRow? role = await res.ReadSingleOrDefaultAsync<RoleRow>();

        if (role is null)
        {
            return null;
        }

        IReadOnlyCollection<string> permissions = [.. await res.ReadAsync<string>()];

        return new Versioned<RoleQueryResponse>(
            new RoleQueryResponse(role.Id, role.OwnerScopeId, role.OwnerScopeName, role.Name, role.IsInheritable, permissions),
            RowVersion.Create(role.RowVersion));
    }

    public async Task<IReadOnlyCollection<AssignableRoleItem>> GetAssignableRolesAsync(Guid scopeId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        // BR-3: owned by the scope, or inheritable and owned by an ancestor; nearest owner first.
        string query =
            """
            SELECT r.id, r.name, r.owner_scope_id AS ownerScopeId, o.name AS ownerScopeName, r.is_inheritable AS isInheritable
            FROM ua.roles r
            JOIN ua.scopes o ON o.id = r.owner_scope_id AND o.deleted_at_utc IS NULL
            JOIN ua.scopes t ON t.id = @ScopeId AND t.deleted_at_utc IS NULL
            WHERE r.deleted_at_utc IS NULL
              AND (r.owner_scope_id = t.id OR (r.is_inheritable AND o.path @> t.path))
            ORDER BY nlevel(o.path) DESC, r.name;
            """;

        return [.. await connection.QueryAsync<AssignableRoleItem>(query, new { ScopeId = scopeId })];
    }

    public async Task<bool> IsNameTakenInLineAsync(Guid ownerScopeId, string name, Guid? excludeRoleId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT EXISTS (
                SELECT 1
                FROM ua.roles r
                JOIN ua.scopes o ON o.id = r.owner_scope_id
                JOIN ua.scopes t ON t.id = @OwnerScopeId
                WHERE r.deleted_at_utc IS NULL AND r.name = @Name
                  AND (@ExcludeId::uuid IS NULL OR r.id <> @ExcludeId)
                  AND (o.path @> t.path OR o.path <@ t.path));
            """;

        return await connection.ExecuteScalarAsync<bool>(query, new { OwnerScopeId = ownerScopeId, Name = name, ExcludeId = excludeRoleId });
    }

    public async Task<bool> IsNameTakenInLineOfRoleAsync(Guid roleId, string name, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT EXISTS (
                SELECT 1
                FROM ua.roles self
                JOIN ua.scopes t ON t.id = self.owner_scope_id
                JOIN ua.roles r ON r.deleted_at_utc IS NULL AND r.name = @Name AND r.id <> self.id
                JOIN ua.scopes o ON o.id = r.owner_scope_id
                WHERE self.id = @RoleId
                  AND (o.path @> t.path OR o.path <@ t.path));
            """;

        return await connection.ExecuteScalarAsync<bool>(query, new { RoleId = roleId, Name = name });
    }

    public async Task<bool> HasLiveAssignmentsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT EXISTS (
                SELECT 1 FROM ua.role_assignments
                WHERE role_id = @RoleId AND revoked_at_utc IS NULL
                  AND (valid_until_utc IS NULL OR valid_until_utc > now()));
            """;

        return await connection.ExecuteScalarAsync<bool>(query, new { RoleId = roleId });
    }

    public async Task<bool> HasLiveAssignmentsAwayFromOwnerAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT EXISTS (
                SELECT 1
                FROM ua.role_assignments a
                JOIN ua.roles r ON r.id = a.role_id
                WHERE a.role_id = @RoleId AND a.scope_id <> r.owner_scope_id AND a.revoked_at_utc IS NULL
                  AND (a.valid_until_utc IS NULL OR a.valid_until_utc > now()));
            """;

        return await connection.ExecuteScalarAsync<bool>(query, new { RoleId = roleId });
    }

    private sealed record RoleRow(Guid Id, Guid OwnerScopeId, string OwnerScopeName, string Name, bool IsInheritable, Guid RowVersion);
}
