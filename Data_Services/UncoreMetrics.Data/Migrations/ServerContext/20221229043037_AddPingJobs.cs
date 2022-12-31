using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UncoreMetrics.Data.Migrations.ServerContext
{
    /// <inheritdoc />
    public partial class AddPingJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LocationID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationName = table.Column<string>(type: "text", nullable: false),
                    ISP = table.Column<string>(type: "text", nullable: false),
                    ASN = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationID);
                });

            migrationBuilder.CreateTable(
                name: "Server_Pings",
                columns: table => new
                {
                    ServerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationID = table.Column<int>(type: "integer", nullable: false),
                    PingMs = table.Column<float>(type: "real", nullable: false),
                    Failed = table.Column<bool>(type: "boolean", nullable: false),
                    LastCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FailedChecks = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Server_Pings", x => new { x.ServerId, x.LocationID });
                    table.ForeignKey(
                        name: "FK_Server_Pings_Servers_ServerId",
                        column: x => x.ServerId,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Server_Pings_ServerId",
                table: "Server_Pings",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Server_Pings_ServerId_LocationID_NextCheck",
                table: "Server_Pings",
                columns: new[] { "ServerId", "LocationID", "NextCheck" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Server_Pings");
        }
    }
}
