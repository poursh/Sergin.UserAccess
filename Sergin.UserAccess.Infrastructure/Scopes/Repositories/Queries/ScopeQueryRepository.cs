using System.Data.Common;
using Sergin.SharedKernel.Application.Concurrency;
using Sergin.SharedKernel.Domain;
using Sergin.SharedKernel.Infrastracture.Data;
using Sergin.UserAccess.Application.Contracts.Scopes.Commands.GetOne;
using Sergin.UserAccess.Application.Scopes;

namespace Sergin.UserAccess.Infrastructure.Scopes.Repositories.Queries;

// Every read filters deleted_at_utc IS NULL itself: the EF soft-delete filter does not reach raw SQL.
internal sealed class ScopeQueryRepository(IDbConnectionFactory connectionFactory) : IScopeQueryRepository
{
    public async Task<Versioned<ScopeQueryResponse>?> GetScopeById(Guid id, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT s.id, s.parent_id AS parentId, s.name, p.name AS parentName, s.row_version AS rowVersion
            FROM ua.scopes s
            LEFT JOIN ua.scopes p ON p.id = s.parent_id
            WHERE s.id = @Id AND s.deleted_at_utc IS NULL;
            """;

        IEnumerable<Versioned<ScopeQueryResponse>> rows = await connection.QueryAsync<ScopeQueryResponse, Guid, Versioned<ScopeQueryResponse>>(
            query,
            (scope, rowVersion) => new Versioned<ScopeQueryResponse>(scope, RowVersion.Create(rowVersion)),
            new { Id = id },
            splitOn: "rowVersion");

        return rows.SingleOrDefault();
    }

    public async Task<IReadOnlyCollection<ScopeTreeRow>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT id, parent_id AS parentId, name, path::text AS path
            FROM ua.scopes
            WHERE deleted_at_utc IS NULL
            ORDER BY nlevel(path), name;
            """;

        return [.. await connection.QueryAsync<ScopeTreeRow>(query)];
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT id, name
            FROM ua.scopes
            WHERE id = ANY(@Ids);
            """;

        IEnumerable<(Guid Id, string Name)> rows = await connection.QueryAsync<(Guid Id, string Name)>(query, new { Ids = ids.ToArray() });

        return rows.ToDictionary(row => row.Id, row => row.Name);
    }

    public async Task<bool> IsSiblingNameTakenAsync(Guid parentId, string name, Guid? excludeScopeId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT EXISTS (
                SELECT 1 FROM ua.scopes
                WHERE parent_id = @ParentId AND name = @Name AND deleted_at_utc IS NULL
                  AND (@ExcludeId::uuid IS NULL OR id <> @ExcludeId));
            """;

        return await connection.ExecuteScalarAsync<bool>(query, new { ParentId = parentId, Name = name, ExcludeId = excludeScopeId });
    }

    public async Task<bool> HasLiveAssignmentsAsync(Guid scopeId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT EXISTS (
                SELECT 1 FROM ua.role_assignments
                WHERE scope_id = @ScopeId AND revoked_at_utc IS NULL
                  AND (valid_until_utc IS NULL OR valid_until_utc > now()));
            """;

        return await connection.ExecuteScalarAsync<bool>(query, new { ScopeId = scopeId });
    }

    public async Task<bool> OwnsLiveRolesAsync(Guid scopeId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        string query =
            """
            SELECT EXISTS (
                SELECT 1 FROM ua.roles
                WHERE owner_scope_id = @ScopeId AND deleted_at_utc IS NULL);
            """;

        return await connection.ExecuteScalarAsync<bool>(query, new { ScopeId = scopeId });
    }

    public async Task<bool> MoveWouldStrandAssignmentsAsync(Guid scopeId, Guid newParentId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync();

        // An assignment inside the moved subtree whose role is owned outside it stays valid only if that owner
        // ends up above it, i.e. is the new parent or one of its ancestors. A role owned inside the subtree moves
        // with it, so its assignments are never stranded.
        string query =
            """
            SELECT EXISTS (
                SELECT 1
                FROM ua.role_assignments a
                JOIN ua.scopes s ON s.id = a.scope_id
                JOIN ua.roles r ON r.id = a.role_id AND r.deleted_at_utc IS NULL
                JOIN ua.scopes o ON o.id = r.owner_scope_id
                JOIN ua.scopes moved ON moved.id = @ScopeId
                JOIN ua.scopes np ON np.id = @NewParentId
                WHERE a.revoked_at_utc IS NULL
                  AND (a.valid_until_utc IS NULL OR a.valid_until_utc > now())
                  AND s.path <@ moved.path
                  AND NOT (o.path <@ moved.path)
                  AND NOT (o.path @> np.path));
            """;

        return await connection.ExecuteScalarAsync<bool>(query, new { ScopeId = scopeId, NewParentId = newParentId });
    }
}
