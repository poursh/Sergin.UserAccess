using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sergin.UserAccess.Infrastructure.Data.Migrations;
/// <inheritdoc />
public partial class AddRolesAndExternalIdentity : Migration
{
    private static readonly Guid AdministratorRoleId = new("01920000-0000-7000-8000-0000000000a1");
    private static readonly Guid ViewerRoleId = new("01920000-0000-7000-8000-0000000000a2");

    private static readonly string[] ViewerPermissions =
    [
        "permission.dm.devices.read",
        "permission.dm.manufacturers.read",
        "permission.ua.users.read",
    ];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "email",
            schema: "ua",
            table: "users",
            type: "character varying(320)",
            maxLength: 320,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "external_id",
            schema: "ua",
            table: "users",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "first_name",
            schema: "ua",
            table: "users",
            type: "character varying(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "last_name",
            schema: "ua",
            table: "users",
            type: "character varying(200)",
            maxLength: 200,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "roles",
            schema: "ua",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_roles", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "user_roles",
            schema: "ua",
            columns: table => new
            {
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                table.ForeignKey(
                    name: "fk_user_roles_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "ua",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "role_permissions",
            schema: "ua",
            columns: table => new
            {
                permission = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_role_permissions", x => new { x.role_id, x.permission });
                table.ForeignKey(
                    name: "fk_role_permissions_roles_role_id",
                    column: x => x.role_id,
                    principalSchema: "ua",
                    principalTable: "roles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_users_external_id",
            schema: "ua",
            table: "users",
            column: "external_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_roles_name",
            schema: "ua",
            table: "roles",
            column: "name",
            unique: true);

        // Seeded rather than left to an admin screen, which this slice does not ship. Without at least
        // one role a just-provisioned user signs in successfully and then sees nothing, with no way
        // inside the product to grant themselves anything. Ids are fixed so the same role means the
        // same row in every environment.
        // One InsertData per row rather than one call with a rectangular values array: CA1814 rejects
        // the multidimensional array the scaffolder would otherwise emit.
        migrationBuilder.InsertData(
            schema: "ua",
            table: "roles",
            columns: ["id", "name"],
            values: [AdministratorRoleId, "administrator"]);

        migrationBuilder.InsertData(
            schema: "ua",
            table: "roles",
            columns: ["id", "name"],
            values: [ViewerRoleId, "viewer"]);

        migrationBuilder.InsertData(
            schema: "ua",
            table: "role_permissions",
            columns: ["role_id", "permission"],
            values: [AdministratorRoleId, "permission.sys.platform-all"]);

        foreach (string permission in ViewerPermissions)
        {
            migrationBuilder.InsertData(
                schema: "ua",
                table: "role_permissions",
                columns: ["role_id", "permission"],
                values: [ViewerRoleId, permission]);
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "role_permissions",
            schema: "ua");

        migrationBuilder.DropTable(
            name: "user_roles",
            schema: "ua");

        migrationBuilder.DropTable(
            name: "roles",
            schema: "ua");

        migrationBuilder.DropIndex(
            name: "ix_users_external_id",
            schema: "ua",
            table: "users");

        migrationBuilder.DropColumn(
            name: "email",
            schema: "ua",
            table: "users");

        migrationBuilder.DropColumn(
            name: "external_id",
            schema: "ua",
            table: "users");

        migrationBuilder.DropColumn(
            name: "first_name",
            schema: "ua",
            table: "users");

        migrationBuilder.DropColumn(
            name: "last_name",
            schema: "ua",
            table: "users");
    }
}
