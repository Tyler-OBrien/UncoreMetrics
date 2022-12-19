using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UncoreMetrics.Data.Migrations.DiscordBotContext
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Discord_Links",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GameServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastStatus = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discord_Links", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Discord_Links_ChannelID",
                table: "Discord_Links",
                column: "ChannelID");

            migrationBuilder.CreateIndex(
                name: "IX_Discord_Links_Enabled",
                table: "Discord_Links",
                column: "Enabled");

            migrationBuilder.CreateIndex(
                name: "IX_Discord_Links_GameServerID",
                table: "Discord_Links",
                column: "GameServerID");

            migrationBuilder.CreateIndex(
                name: "IX_Discord_Links_ServerID",
                table: "Discord_Links",
                column: "ServerID");

            migrationBuilder.CreateIndex(
                name: "IX_Discord_Links_UserID",
                table: "Discord_Links",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Discord_Links");
        }
    }
}
