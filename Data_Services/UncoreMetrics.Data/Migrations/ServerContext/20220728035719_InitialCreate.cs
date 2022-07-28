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
                    IpAddressBytes = table.Column<byte[]>(type: "bytea", maxLength: 16, nullable: false),
                    Address = table.Column<IPAddress>(type: "inet", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    QueryPort = table.Column<int>(type: "integer", nullable: false),
                    Players = table.Column<long>(type: "bigint", nullable: false),
                    MaxPlayers = table.Column<long>(type: "bigint", nullable: false),
                    Visibility = table.Column<bool>(type: "boolean", nullable: false),
                    Environment = table.Column<byte>(type: "smallint", nullable: false),
                    VAC = table.Column<bool>(type: "boolean", nullable: false),
                    Keywords = table.Column<string>(type: "text", nullable: true),
                    SteamID = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
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
                name: "Arma3_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arma3_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_Arma3_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DayZ_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowedBuild = table.Column<bool>(type: "boolean", nullable: true),
                    Island = table.Column<string>(type: "text", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: true),
                    RequiredBuild = table.Column<int>(type: "integer", nullable: true),
                    RequiredVersion = table.Column<int>(type: "integer", nullable: true),
                    TimeLeft = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayZ_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_DayZ_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HellLetLoose_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionFlags = table.Column<int>(type: "integer", nullable: true),
                    Visible = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HellLetLoose_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_HellLetLoose_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostScriptsum_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    AllModsWhitelisted = table.Column<bool>(type: "boolean", nullable: true),
                    CurrentModLoadedCount = table.Column<int>(type: "integer", nullable: true),
                    Flags = table.Column<int>(type: "integer", nullable: true),
                    GameMode = table.Column<string>(type: "text", nullable: true),
                    GameVersion = table.Column<string>(type: "text", nullable: true),
                    MatchTimeout = table.Column<int>(type: "integer", nullable: true),
                    Password = table.Column<bool>(type: "boolean", nullable: true),
                    PlayerReserveCount = table.Column<int>(type: "integer", nullable: true),
                    PublicQueue = table.Column<int>(type: "integer", nullable: true),
                    ReservedQueue = table.Column<int>(type: "integer", nullable: true),
                    SearchKeywords = table.Column<string>(type: "text", nullable: true),
                    SessionFlags = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostScriptsum_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_PostScriptsum_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectZomboid_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ModCount = table.Column<int>(type: "integer", nullable: true),
                    Mods = table.Column<string>(type: "text", nullable: true),
                    Open = table.Column<bool>(type: "boolean", nullable: true),
                    Public = table.Column<bool>(type: "boolean", nullable: true),
                    PvP = table.Column<bool>(type: "boolean", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectZomboid_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_ProjectZomboid_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rust_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    Build = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    EntityCount = table.Column<int>(type: "integer", nullable: true),
                    FPS = table.Column<int>(type: "integer", nullable: true),
                    AverageFPS = table.Column<int>(type: "integer", nullable: true),
                    gc_cl = table.Column<int>(type: "integer", nullable: true),
                    gc_mb = table.Column<int>(type: "integer", nullable: true),
                    Hash = table.Column<string>(type: "text", nullable: true),
                    HeaderImage = table.Column<string>(type: "text", nullable: true),
                    LogoImage = table.Column<string>(type: "text", nullable: true),
                    PvE = table.Column<bool>(type: "boolean", nullable: true),
                    Uptime = table.Column<int>(type: "integer", nullable: true),
                    URL = table.Column<string>(type: "text", nullable: true),
                    WorldSeed = table.Column<int>(type: "integer", nullable: true),
                    WorldSize = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rust_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_Rust_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SevenDTD_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    AirDropMarker = table.Column<bool>(type: "boolean", nullable: true),
                    BedrollExpiryTime = table.Column<int>(type: "integer", nullable: true),
                    BloodMoonFrequency = table.Column<string>(type: "text", nullable: true),
                    BuildCreate = table.Column<bool>(type: "boolean", nullable: true),
                    CompatibilityVersion = table.Column<string>(type: "text", nullable: true),
                    CurrentServerTime = table.Column<int>(type: "integer", nullable: true),
                    DayCount = table.Column<int>(type: "integer", nullable: true),
                    DropOnDeath = table.Column<bool>(type: "boolean", nullable: true),
                    DropOnQuit = table.Column<bool>(type: "boolean", nullable: true),
                    EACEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    EnemyDifficulty = table.Column<int>(type: "integer", nullable: true),
                    GameDifficulty = table.Column<int>(type: "integer", nullable: true),
                    GameHost = table.Column<string>(type: "text", nullable: true),
                    GameName = table.Column<string>(type: "text", nullable: true),
                    IsPasswordProtected = table.Column<bool>(type: "boolean", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: true),
                    LandClaimCount = table.Column<int>(type: "integer", nullable: true),
                    LandClaimDecayMode = table.Column<int>(type: "integer", nullable: true),
                    LandClaimExpiryTime = table.Column<int>(type: "integer", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: true),
                    LevelName = table.Column<string>(type: "text", nullable: true),
                    LootAbundance = table.Column<int>(type: "integer", nullable: true),
                    LootRespawnDays = table.Column<int>(type: "integer", nullable: true),
                    MaxSpawnedAnimals = table.Column<int>(type: "integer", nullable: true),
                    MaxSpawnedZombies = table.Column<int>(type: "integer", nullable: true),
                    ModdedConfig = table.Column<bool>(type: "boolean", nullable: true),
                    PlayerKillingMode = table.Column<int>(type: "integer", nullable: true),
                    Region = table.Column<string>(type: "text", nullable: true),
                    RequiresMod = table.Column<bool>(type: "boolean", nullable: true),
                    ServerDescription = table.Column<string>(type: "text", nullable: true),
                    ServerLoginConfirmationText = table.Column<string>(type: "text", nullable: true),
                    ServerWebsiteURL = table.Column<string>(type: "text", nullable: true),
                    ShowFriendPlayerOnMap = table.Column<bool>(type: "boolean", nullable: true),
                    StockFiles = table.Column<bool>(type: "boolean", nullable: true),
                    StockSettings = table.Column<bool>(type: "boolean", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: true),
                    WorldSize = table.Column<int>(type: "integer", nullable: true),
                    XPMultiplier = table.Column<int>(type: "integer", nullable: true),
                    ZombieBMMove = table.Column<int>(type: "integer", nullable: true),
                    ZombieFeralMove = table.Column<int>(type: "integer", nullable: true),
                    ZombieFeralSense = table.Column<int>(type: "integer", nullable: true),
                    ZombieMove = table.Column<int>(type: "integer", nullable: true),
                    ZombieMoveNight = table.Column<int>(type: "integer", nullable: true),
                    ZombiesRun = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SevenDTD_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_SevenDTD_Servers_Servers_ServerID",
                        column: x => x.ServerID,
                        principalTable: "Servers",
                        principalColumn: "ServerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Unturned_Servers",
                columns: table => new
                {
                    ServerID = table.Column<Guid>(type: "uuid", nullable: false),
                    BrowserDescription = table.Column<string>(type: "text", nullable: true),
                    BrowserDescriptionHint = table.Column<string>(type: "text", nullable: true),
                    BrowserIcon = table.Column<string>(type: "text", nullable: true),
                    CustomLinks = table.Column<List<string>>(type: "text[]", nullable: true),
                    GameVersion = table.Column<string>(type: "text", nullable: true),
                    Mods = table.Column<string>(type: "text", nullable: true),
                    RocketPlugins = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unturned_Servers", x => x.ServerID);
                    table.ForeignKey(
                        name: "FK_Unturned_Servers_Servers_ServerID",
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
                name: "IX_DayZ_Servers_AllowedBuild",
                table: "DayZ_Servers",
                column: "AllowedBuild");

            migrationBuilder.CreateIndex(
                name: "IX_DayZ_Servers_Island",
                table: "DayZ_Servers",
                column: "Island");

            migrationBuilder.CreateIndex(
                name: "IX_HellLetLoose_Servers_Visible",
                table: "HellLetLoose_Servers",
                column: "Visible");

            migrationBuilder.CreateIndex(
                name: "IX_PostScriptsum_Servers_CurrentModLoadedCount",
                table: "PostScriptsum_Servers",
                column: "CurrentModLoadedCount");

            migrationBuilder.CreateIndex(
                name: "IX_PostScriptsum_Servers_GameMode",
                table: "PostScriptsum_Servers",
                column: "GameMode");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectZomboid_Servers_Open",
                table: "ProjectZomboid_Servers",
                column: "Open");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectZomboid_Servers_PvP",
                table: "ProjectZomboid_Servers",
                column: "PvP");

            migrationBuilder.CreateIndex(
                name: "IX_Rust_Servers_EntityCount",
                table: "Rust_Servers",
                column: "EntityCount");

            migrationBuilder.CreateIndex(
                name: "IX_Rust_Servers_PvE",
                table: "Rust_Servers",
                column: "PvE");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_AppID",
                table: "Servers",
                column: "AppID");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Continent",
                table: "Servers",
                column: "Continent");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_IpAddressBytes_Port",
                table: "Servers",
                columns: new[] { "IpAddressBytes", "Port" },
                unique: true);

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
                name: "IX_SevenDTD_Servers_DayCount",
                table: "SevenDTD_Servers",
                column: "DayCount");

            migrationBuilder.CreateIndex(
                name: "IX_SevenDTD_Servers_EACEnabled",
                table: "SevenDTD_Servers",
                column: "EACEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_SevenDTD_Servers_IsPasswordProtected",
                table: "SevenDTD_Servers",
                column: "IsPasswordProtected");

            migrationBuilder.CreateIndex(
                name: "IX_SevenDTD_Servers_RequiresMod",
                table: "SevenDTD_Servers",
                column: "RequiresMod");

            migrationBuilder.CreateIndex(
                name: "IX_SevenDTD_Servers_Version",
                table: "SevenDTD_Servers",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Unturned_Servers_Mods",
                table: "Unturned_Servers",
                column: "Mods");

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
                name: "Arma3_Servers");

            migrationBuilder.DropTable(
                name: "DayZ_Servers");

            migrationBuilder.DropTable(
                name: "HellLetLoose_Servers");

            migrationBuilder.DropTable(
                name: "PostScriptsum_Servers");

            migrationBuilder.DropTable(
                name: "ProjectZomboid_Servers");

            migrationBuilder.DropTable(
                name: "Rust_Servers");

            migrationBuilder.DropTable(
                name: "SevenDTD_Servers");

            migrationBuilder.DropTable(
                name: "Unturned_Servers");

            migrationBuilder.DropTable(
                name: "V_Rising_Servers");

            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}
