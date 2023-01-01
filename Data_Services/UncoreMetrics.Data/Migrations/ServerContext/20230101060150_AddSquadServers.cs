using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UncoreMetrics.Data.Migrations.ServerContext
{
    /// <inheritdoc />
    public partial class AddSquadServers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Squad_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    Flags = table.Column<int>(type: "integer", nullable: true),
                    GameMode = table.Column<string>(type: "text", nullable: true),
                    GameVersion = table.Column<string>(type: "text", nullable: true),
                    OPENPRIVCONN = table.Column<int>(type: "integer", nullable: true),
                    NUMOPENPUBCONN = table.Column<int>(type: "integer", nullable: true),
                    NUMPRIVCONN = table.Column<int>(type: "integer", nullable: true),
                    NUMPUBCONN = table.Column<int>(type: "integer", nullable: true),
                    HasPassword = table.Column<bool>(type: "boolean", nullable: true),
                    PlayerCount = table.Column<int>(type: "integer", nullable: true),
                    PlayerReserveCount = table.Column<int>(type: "integer", nullable: true),
                    PublicQueueLimit = table.Column<int>(type: "integer", nullable: true),
                    PublicQueue = table.Column<int>(type: "integer", nullable: true),
                    ReservedQueue = table.Column<int>(type: "integer", nullable: true),
                    SEARCHKEYWORDS = table.Column<string>(type: "text", nullable: true),
                    SESSIONFLAGS = table.Column<int>(type: "integer", nullable: true),
                    TeamOne = table.Column<string>(type: "text", nullable: true),
                    TeamTwo = table.Column<string>(type: "text", nullable: true),
                    LicenseID = table.Column<int>(type: "integer", nullable: true),
                    LicenseSig = table.Column<string>(type: "text", nullable: true),
                    ValidLicense = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Squad_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_Squad_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Squad_Servers_GameMode",
                table: "Squad_Servers",
                column: "GameMode");

            migrationBuilder.CreateIndex(
                name: "IX_Squad_Servers_GameVersion",
                table: "Squad_Servers",
                column: "GameVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Squad_Servers_HasPassword",
                table: "Squad_Servers",
                column: "HasPassword");

            migrationBuilder.CreateIndex(
                name: "IX_Squad_Servers_ValidLicense",
                table: "Squad_Servers",
                column: "ValidLicense");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Squad_Servers");
        }
    }
}
