using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AStarOneDriveClient.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxParallelAndBatchSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxItemsInBatch",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxParallelUpDownloads",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxItemsInBatch",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "MaxParallelUpDownloads",
                table: "Accounts");
        }
    }
}
