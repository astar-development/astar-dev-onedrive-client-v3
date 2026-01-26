using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AStar.Dev.OneDrive.Client.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSelectedFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<string>(type: "TEXT", nullable: false),
                    FolderPath = table.Column<string>(type: "TEXT", nullable: false),
                    IsSelected = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncConfigurations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncConfigurations_AccountId_FolderPath",
                table: "SyncConfigurations",
                columns: new[] { "AccountId", "FolderPath" });
        }
    }
}
