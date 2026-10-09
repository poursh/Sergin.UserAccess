using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sergin.UserAccess.Infrastructure.Data.Migrations;

/// <summary>
/// Scoped RBAC. Adds the scope tree (ltree paths) with one seeded root, makes every role owned by a scope, turns
/// <c>user_roles</c> into <c>role_assignments</c>, and adds the append-only authorization audit log.
/// Existing data keeps its meaning exactly: every existing role becomes owned by the root and inheritable — a
/// global role (BR-7) — and every existing user-role row becomes an assignment at the root over the whole tree,
/// so every user holds the same permissions everywhere as before. Audit stamps for existing rows use the
/// migration time and the platform's fixed system actor (the outbox relay identity's id), as in dm's
/// AddAuditColumns.
/// </summary>
public partial class AddScopedRbac : Migration
{
    private const string Schema = "ua";
    private const string RootId = "01920000-0000-7000-8000-0000000000b1";
    private const string SystemActor = "01920000-0000-7000-8000-00000000000f";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:PostgresExtension:ltree", ",,");

        CreateScopes(migrationBuilder);
        OwnRolesByRoot(migrationBuilder);
        CreateAssignmentsFromUserRoles(migrationBuilder);
        CreateAuditLog(migrationBuilder);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "user_roles",
            schema: Schema,
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                table.ForeignKey(
                    name: "fk_user_roles_users_user_id",
                    column: x => x.user_id,
                    principalSchema: Schema,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Only what the old model could express: live global assignments of live root roles.
        migrationBuilder.Sql(
            $"""
            INSERT INTO ua.user_roles (user_id, role_id)
            SELECT DISTINCT a.user_id, a.role_id
            FROM ua.role_assignments a
            JOIN ua.roles r ON r.id = a.role_id AND r.deleted_at_utc IS NULL AND r.owner_scope_id = '{RootId}'
            WHERE a.scope_id = '{RootId}' AND a.include_subtree AND a.revoked_at_utc IS NULL;
            """);

        migrationBuilder.Sql("DELETE FROM ua.roles WHERE deleted_at_utc IS NOT NULL OR owner_scope_id <> '" + RootId + "';");

        migrationBuilder.DropForeignKey(name: "fk_roles_scopes_owner_scope_id", schema: Schema, table: "roles");
        migrationBuilder.DropTable(name: "authorization_audit_log", schema: Schema);
        migrationBuilder.Sql("DROP FUNCTION ua.refuse_audit_log_change();");
        migrationBuilder.DropTable(name: "role_assignments", schema: Schema);
        migrationBuilder.DropTable(name: "scopes", schema: Schema);

        migrationBuilder.DropIndex(name: "ix_roles_owner_scope_id_name", schema: Schema, table: "roles");
        migrationBuilder.DropCheckConstraint(name: "ck_roles_soft_delete", schema: Schema, table: "roles");

        foreach (string column in new[]
        {
            "created_at_utc", "created_by", "deleted_at_utc", "deleted_by", "is_inheritable",
            "modified_at_utc", "modified_by", "owner_scope_id", "row_version"
        })
        {
            migrationBuilder.DropColumn(name: column, schema: Schema, table: "roles");
        }

        foreach (string column in new[] { "created_at_utc", "created_by", "modified_at_utc", "modified_by" })
        {
            migrationBuilder.DropColumn(name: column, schema: Schema, table: "role_permissions");
        }

        migrationBuilder.CreateIndex(name: "ix_roles_name", schema: Schema, table: "roles", column: "name", unique: true);

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");
    }

    private static void CreateScopes(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "scopes",
            schema: Schema,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                path = table.Column<string>(type: "ltree", nullable: false),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                modified_by = table.Column<Guid>(type: "uuid", nullable: true),
                row_version = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_scopes", x => x.id);
                table.CheckConstraint("ck_scopes_soft_delete", "(deleted_at_utc IS NULL) = (deleted_by IS NULL)");
                table.ForeignKey(
                    name: "fk_scopes_scopes_parent_id",
                    column: x => x.parent_id,
                    principalSchema: Schema,
                    principalTable: "scopes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateIndex(
            name: "ix_scopes_parent_id_name",
            schema: Schema,
            table: "scopes",
            columns: ["parent_id", "name"],
            unique: true,
            filter: "deleted_at_utc IS NULL");

        migrationBuilder.CreateIndex(
                name: "ix_scopes_path",
                schema: Schema,
                table: "scopes",
                column: "path")
            .Annotation("Npgsql:IndexMethod", "gist");

        // The single root (Scope.RootId). The tree has one, and nothing in the application creates another.
        migrationBuilder.Sql(
            $"""
            INSERT INTO ua.scopes (id, name, parent_id, path, created_at_utc, created_by, row_version)
            VALUES ('{RootId}', 'Root', NULL, replace('{RootId}', '-', '')::ltree, now(), '{SystemActor}', gen_random_uuid());
            """);
    }

    private static void OwnRolesByRoot(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "ix_roles_name", schema: Schema, table: "roles");

        // Added nullable, backfilled, then made NOT NULL — existing rows need a value before the constraint.
        migrationBuilder.AddColumn<Guid>(name: "owner_scope_id", schema: Schema, table: "roles", type: "uuid", nullable: true);
        migrationBuilder.AddColumn<bool>(name: "is_inheritable", schema: Schema, table: "roles", type: "boolean", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<Guid>(name: "row_version", schema: Schema, table: "roles", type: "uuid", nullable: true);
        AddAuditColumns(migrationBuilder, "roles");
        migrationBuilder.AddColumn<DateTime>(name: "deleted_at_utc", schema: Schema, table: "roles", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "deleted_by", schema: Schema, table: "roles", type: "uuid", nullable: true);
        AddAuditColumns(migrationBuilder, "role_permissions");

        // Every existing role was global; root-owned and inheritable is exactly that (BR-7).
        migrationBuilder.Sql(
            $"""
            UPDATE ua.roles
            SET owner_scope_id = '{RootId}', is_inheritable = true, row_version = gen_random_uuid(),
                created_at_utc = now(), created_by = '{SystemActor}';
            UPDATE ua.role_permissions SET created_at_utc = now(), created_by = '{SystemActor}';
            """);

        RequireUuid(migrationBuilder, "roles", "owner_scope_id");
        RequireUuid(migrationBuilder, "roles", "row_version");
        RequireCreated(migrationBuilder, "roles");
        RequireCreated(migrationBuilder, "role_permissions");

        migrationBuilder.AddCheckConstraint(
            name: "ck_roles_soft_delete",
            schema: Schema,
            table: "roles",
            sql: "(deleted_at_utc IS NULL) = (deleted_by IS NULL)");

        migrationBuilder.CreateIndex(
            name: "ix_roles_owner_scope_id_name",
            schema: Schema,
            table: "roles",
            columns: ["owner_scope_id", "name"],
            unique: true,
            filter: "deleted_at_utc IS NULL");

        migrationBuilder.AddForeignKey(
            name: "fk_roles_scopes_owner_scope_id",
            schema: Schema,
            table: "roles",
            column: "owner_scope_id",
            principalSchema: Schema,
            principalTable: "scopes",
            principalColumn: "id",
            onDelete: ReferentialAction.NoAction);
    }

    private static void CreateAssignmentsFromUserRoles(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "role_assignments",
            schema: Schema,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_id = table.Column<Guid>(type: "uuid", nullable: false),
                include_subtree = table.Column<bool>(type: "boolean", nullable: false),
                valid_from_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                valid_until_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                revoked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                revoked_by = table.Column<Guid>(type: "uuid", nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_role_assignments", x => x.id);
                table.ForeignKey(
                    name: "fk_role_assignments_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: Schema,
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "fk_role_assignments_scopes_scope_id",
                    column: x => x.scope_id,
                    principalSchema: Schema,
                    principalTable: "scopes",
                    principalColumn: "id",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "fk_role_assignments_users_user_id",
                    column: x => x.user_id,
                    principalSchema: Schema,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "ix_role_assignments_role_id", schema: Schema, table: "role_assignments", column: "role_id");
        migrationBuilder.CreateIndex(name: "ix_role_assignments_scope_id", schema: Schema, table: "role_assignments", column: "scope_id");
        migrationBuilder.CreateIndex(
            name: "ix_role_assignments_user_id",
            schema: Schema,
            table: "role_assignments",
            column: "user_id",
            filter: "revoked_at_utc IS NULL");

        // Every old user-role row becomes a global assignment. user_roles had no key to roles, so a row naming
        // a role that no longer exists is dropped rather than failing the new foreign key.
        migrationBuilder.Sql(
            $"""
            INSERT INTO ua.role_assignments (id, user_id, role_id, scope_id, include_subtree)
            SELECT gen_random_uuid(), ur.user_id, ur.role_id, '{RootId}', true
            FROM ua.user_roles ur
            JOIN ua.roles r ON r.id = ur.role_id;
            """);

        migrationBuilder.DropTable(name: "user_roles", schema: Schema);
    }

    private static void CreateAuditLog(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "authorization_audit_log",
            schema: Schema,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_id = table.Column<Guid>(type: "uuid", nullable: true),
                details = table.Column<string>(type: "jsonb", nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_authorization_audit_log", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_authorization_audit_log_entity_id", schema: Schema, table: "authorization_audit_log", column: "entity_id");
        migrationBuilder.CreateIndex(
            name: "ix_authorization_audit_log_occurred_at_utc", schema: Schema, table: "authorization_audit_log", column: "occurred_at_utc");

        // NFR-6: the log cannot be edited. Refused at the database, so neither the application nor a
        // hand-written statement can rewrite or remove an entry; 42501 is insufficient_privilege.
        migrationBuilder.Sql(
            """
            CREATE FUNCTION ua.refuse_audit_log_change() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
                RAISE EXCEPTION 'ua.authorization_audit_log is append-only' USING ERRCODE = '42501';
            END;
            $$;

            CREATE TRIGGER authorization_audit_log_append_only
                BEFORE UPDATE OR DELETE ON ua.authorization_audit_log
                FOR EACH ROW EXECUTE FUNCTION ua.refuse_audit_log_change();

            CREATE TRIGGER authorization_audit_log_no_truncate
                BEFORE TRUNCATE ON ua.authorization_audit_log
                FOR EACH STATEMENT EXECUTE FUNCTION ua.refuse_audit_log_change();
            """);
    }

    private static void AddAuditColumns(MigrationBuilder migrationBuilder, string table)
    {
        migrationBuilder.AddColumn<DateTime>(name: "created_at_utc", schema: Schema, table: table, type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "created_by", schema: Schema, table: table, type: "uuid", nullable: true);
        migrationBuilder.AddColumn<DateTime>(name: "modified_at_utc", schema: Schema, table: table, type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<Guid>(name: "modified_by", schema: Schema, table: table, type: "uuid", nullable: true);
    }

    private static void RequireCreated(MigrationBuilder migrationBuilder, string table)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "created_at_utc", schema: Schema, table: table, type: "timestamp with time zone", nullable: false,
            oldClrType: typeof(DateTime), oldType: "timestamp with time zone", oldNullable: true);

        RequireUuid(migrationBuilder, table, "created_by");
    }

    private static void RequireUuid(MigrationBuilder migrationBuilder, string table, string column) =>
        migrationBuilder.AlterColumn<Guid>(
            name: column, schema: Schema, table: table, type: "uuid", nullable: false,
            oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);
}
