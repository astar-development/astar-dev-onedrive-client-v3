using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AStarOneDriveClient.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoSyncIntervalMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AutoSyncIntervalMinutes",
                table: "Accounts",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoSyncIntervalMinutes",
                table: "Accounts");
        }
    }
}
