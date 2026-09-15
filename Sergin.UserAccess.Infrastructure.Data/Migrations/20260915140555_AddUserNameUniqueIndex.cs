using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sergin.UserAccess.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddUserNameUniqueIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_users_user_name",
            schema: "ua",
            table: "users",
            column: "user_name",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_users_user_name",
            schema: "ua",
            table: "users");
    }
}
