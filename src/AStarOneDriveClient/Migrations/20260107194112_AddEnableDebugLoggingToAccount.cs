using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AStarOneDriveClient.Migrations;

/// <inheritdoc />
public partial class AddEnableDebugLoggingToAccount : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "EnableDebugLogging",
            table: "Accounts",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EnableDebugLogging",
            table: "Accounts");
    }
}
