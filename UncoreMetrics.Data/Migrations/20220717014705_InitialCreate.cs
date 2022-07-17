using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace UncoreMetrics.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    IpAddressBytes = table.Column<byte[]>(type: "bytea", maxLength: 16, nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "Name" }),
                    Game = table.Column<string>(type: "text", nullable: false),
                    AppID = table.Column<long>(type: "bigint", nullable: false),
                    Address = table.Column<IPAddress>(type: "inet", nullable: false),
                    QueryPort = table.Column<int>(type: "integer", nullable: false),
                    Players = table.Column<int>(type: "integer", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    ASN = table.Column<long>(type: "bigint", nullable: true),
                    ISP = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    Continent = table.Column<string>(type: "text", nullable: true),
                    Timezone = table.Column<string>(type: "text", nullable: true),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    ServerDead = table.Column<bool>(type: "boolean", nullable: false),
                    LastCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FailedChecks = table.Column<int>(type: "integer", nullable: false),
                    FoundAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => new { x.IpAddressBytes, x.Port });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Servers_AppID",
                table: "Servers",
                column: "AppID");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Continent",
                table: "Servers",
                column: "Continent");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_IsOnline",
                table: "Servers",
                column: "IsOnline");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_NextCheck",
                table: "Servers",
                column: "NextCheck");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_SearchVector",
                table: "Servers",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_ServerDead",
                table: "Servers",
                column: "ServerDead");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
