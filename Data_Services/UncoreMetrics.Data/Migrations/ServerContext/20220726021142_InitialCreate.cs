using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace UncoreMetrics.Data.Migrations.ServerContext
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                        .Annotation("Npgsql:TsVectorConfig", "english")
                        .Annotation("Npgsql:TsVectorProperties", new[] { "Name" }),
                    Game = table.Column<string>(type: "text", nullable: false),
                    AppID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IpAddressBytes = table.Column<byte[]>(type: "bytea", maxLength: 16, nullable: true),
                    Address = table.Column<IPAddress>(type: "inet", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    QueryPort = table.Column<int>(type: "integer", nullable: false),
                    Players = table.Column<long>(type: "bigint", nullable: false),
                    MaxPlayers = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_Servers", x => x.ServerID);
                });

            migrationBuilder.CreateTable(
                name: "Ark_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    Modded = table.Column<bool>(type: "boolean", nullable: true),
                    DownloadCharacters = table.Column<bool>(type: "boolean", nullable: true),
                    DownloadItems = table.Column<bool>(type: "boolean", nullable: true),
                    Mods = table.Column<List<string>>(type: "text[]", nullable: true),
                    DaysRunning = table.Column<int>(type: "integer", nullable: true),
                    SessionFlags = table.Column<int>(type: "integer", nullable: true),
                    ClusterID = table.Column<string>(type: "text", nullable: true),
                    CustomServerName = table.Column<string>(type: "text", nullable: true),
                    PasswordRequired = table.Column<bool>(type: "boolean", nullable: true),
                    Battleye = table.Column<bool>(type: "boolean", nullable: true),
                    OfficialServer = table.Column<bool>(type: "boolean", nullable: true),
                    PVE = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ark_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_Ark_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "V_Rising_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    HeartDamage = table.Column<int>(type: "integer", nullable: true),
                    BloodBoundEquipment = table.Column<bool>(type: "boolean", nullable: true),
                    DaysRunning = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_V_Rising_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_V_Rising_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ark_Servers_Battleye",
                table: "Ark_Servers",
                column: "Battleye");

            migrationBuilder.CreateIndex(
                name: "IX_Ark_Servers_PasswordRequired",
                table: "Ark_Servers",
                column: "PasswordRequired");

            migrationBuilder.CreateIndex(
                name: "IX_Ark_Servers_PVE",
                table: "Ark_Servers",
                column: "PVE");

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

            migrationBuilder.CreateIndex(
                name: "IX_V_Rising_Servers_BloodBoundEquipment",
                table: "V_Rising_Servers",
                column: "BloodBoundEquipment");

            migrationBuilder.CreateIndex(
                name: "IX_V_Rising_Servers_HeartDamage",
                table: "V_Rising_Servers",
                column: "HeartDamage");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ark_Servers");

            migrationBuilder.DropTable(
                name: "V_Rising_Servers");

            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
