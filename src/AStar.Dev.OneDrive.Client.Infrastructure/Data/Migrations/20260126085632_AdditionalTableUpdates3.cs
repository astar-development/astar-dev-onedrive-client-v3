using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AStar.Dev.OneDrive.Client.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalTableUpdates3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriveItems_AccountId",
                table: "DriveItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "DriveItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DriveItems_AccountId_RelativePath",
                table: "DriveItems",
                columns: new[] { "AccountId", "RelativePath" });

            migrationBuilder.CreateIndex(
                name: "IX_DriveItems_IsFolder",
                table: "DriveItems",
                column: "IsFolder");

            migrationBuilder.CreateIndex(
                name: "IX_DriveItems_IsSelected",
                table: "DriveItems",
                column: "IsSelected");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriveItems_AccountId_RelativePath",
                table: "DriveItems");

            migrationBuilder.DropIndex(
                name: "IX_DriveItems_IsFolder",
                table: "DriveItems");

            migrationBuilder.DropIndex(
                name: "IX_DriveItems_IsSelected",
                table: "DriveItems");

            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "DriveItems");

            migrationBuilder.CreateIndex(
                name: "IX_DriveItems_AccountId",
                table: "DriveItems",
                column: "AccountId");
        }
    }
}
