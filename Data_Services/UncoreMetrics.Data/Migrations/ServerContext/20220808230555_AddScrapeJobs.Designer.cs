﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;
using UncoreMetrics.Data;

#nullable disable

namespace UncoreMetrics.Data.Migrations.ServerContext
{
    [DbContext(typeof(ServersContext))]
    [Migration("20220808230555_AddScrapeJobs")]
    partial class AddScrapeJobs
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("UncoreMetrics.Data.ScrapeJob", b =>
                {
                    b.Property<string>("InternalId")
                        .HasColumnType("text");

                    b.Property<string>("GameType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUpdateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Node")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Progress")
                        .HasColumnType("integer");

                    b.Property<Guid>("RunGuid")
                        .HasColumnType("uuid");

                    b.Property<int>("RunId")
                        .HasColumnType("integer");

                    b.Property<string>("RunType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Running")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TotalCount")
                        .HasColumnType("integer");

                    b.Property<int>("TotalDone")
                        .HasColumnType("integer");

                    b.HasKey("InternalId");

                    b.ToTable("Scrape_Jobs", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.Server", b =>
                {
                    b.Property<Guid>("ServerID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long?>("ASN")
                        .HasColumnType("bigint");

                    b.Property<IPAddress>("Address")
                        .IsRequired()
                        .HasColumnType("inet");

                    b.Property<decimal>("AppID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("Continent")
                        .HasColumnType("integer");

                    b.Property<string>("Country")
                        .HasColumnType("text");

                    b.Property<char?>("Environment")
                        .HasColumnType("character(1)");

                    b.Property<int>("FailedChecks")
                        .HasColumnType("integer");

                    b.Property<DateTime>("FoundAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Game")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ISP")
                        .HasColumnType("text");

                    b.Property<byte[]>("IpAddressBytes")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("bytea");

                    b.Property<bool>("IsOnline")
                        .HasColumnType("boolean");

                    b.Property<string>("Keywords")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastCheck")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double?>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double?>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Map")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("MaxPlayers")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("NextCheck")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("Players")
                        .HasColumnType("bigint");

                    b.Property<int>("Port")
                        .HasColumnType("integer");

                    b.Property<int>("QueryPort")
                        .HasColumnType("integer");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tsvector")
                        .HasAnnotation("Npgsql:TsVectorConfig", "english")
                        .HasAnnotation("Npgsql:TsVectorProperties", new[] { "Name" });

                    b.Property<bool>("ServerDead")
                        .HasColumnType("boolean");

                    b.Property<decimal?>("SteamID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Timezone")
                        .HasColumnType("text");

                    b.Property<bool?>("VAC")
                        .HasColumnType("boolean");

                    b.Property<bool?>("Visibility")
                        .HasColumnType("boolean");

                    b.HasKey("ServerID");

                    b.HasIndex("AppID");

                    b.HasIndex("Continent");

                    b.HasIndex("IsOnline");

                    b.HasIndex("NextCheck");

                    b.HasIndex("SearchVector");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("SearchVector"), "GIN");

                    b.HasIndex("ServerDead");

                    b.HasIndex("IpAddressBytes", "QueryPort")
                        .IsUnique();

                    b.ToTable("Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData._7DaysToDie.SevenDaysToDieServer", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<bool?>("AirDropMarker")
                        .HasColumnType("boolean");

                    b.Property<int?>("BedrollExpiryTime")
                        .HasColumnType("integer");

                    b.Property<string>("BloodMoonFrequency")
                        .HasColumnType("text");

                    b.Property<bool?>("BuildCreate")
                        .HasColumnType("boolean");

                    b.Property<string>("CompatibilityVersion")
                        .HasColumnType("text");

                    b.Property<int?>("CurrentServerTime")
                        .HasColumnType("integer");

                    b.Property<int?>("DayCount")
                        .HasColumnType("integer");

                    b.Property<bool?>("DropOnDeath")
                        .HasColumnType("boolean");

                    b.Property<bool?>("DropOnQuit")
                        .HasColumnType("boolean");

                    b.Property<bool?>("EACEnabled")
                        .HasColumnType("boolean");

                    b.Property<int?>("EnemyDifficulty")
                        .HasColumnType("integer");

                    b.Property<int?>("GameDifficulty")
                        .HasColumnType("integer");

                    b.Property<string>("GameHost")
                        .HasColumnType("text");

                    b.Property<string>("GameName")
                        .HasColumnType("text");

                    b.Property<bool?>("IsPasswordProtected")
                        .HasColumnType("boolean");

                    b.Property<bool?>("IsPublic")
                        .HasColumnType("boolean");

                    b.Property<int?>("LandClaimCount")
                        .HasColumnType("integer");

                    b.Property<int?>("LandClaimDecayMode")
                        .HasColumnType("integer");

                    b.Property<int?>("LandClaimExpiryTime")
                        .HasColumnType("integer");

                    b.Property<string>("Language")
                        .HasColumnType("text");

                    b.Property<string>("LevelName")
                        .HasColumnType("text");

                    b.Property<int?>("LootAbundance")
                        .HasColumnType("integer");

                    b.Property<int?>("LootRespawnDays")
                        .HasColumnType("integer");

                    b.Property<int?>("MaxSpawnedAnimals")
                        .HasColumnType("integer");

                    b.Property<int?>("MaxSpawnedZombies")
                        .HasColumnType("integer");

                    b.Property<bool?>("ModdedConfig")
                        .HasColumnType("boolean");

                    b.Property<int?>("PlayerKillingMode")
                        .HasColumnType("integer");

                    b.Property<string>("Region")
                        .HasColumnType("text");

                    b.Property<bool?>("RequiresMod")
                        .HasColumnType("boolean");

                    b.Property<string>("ServerDescription")
                        .HasColumnType("text");

                    b.Property<string>("ServerLoginConfirmationText")
                        .HasColumnType("text");

                    b.Property<string>("ServerWebsiteURL")
                        .HasColumnType("text");

                    b.Property<bool?>("ShowFriendPlayerOnMap")
                        .HasColumnType("boolean");

                    b.Property<bool?>("StockFiles")
                        .HasColumnType("boolean");

                    b.Property<bool?>("StockSettings")
                        .HasColumnType("boolean");

                    b.Property<string>("Version")
                        .HasColumnType("text");

                    b.Property<int?>("WorldSize")
                        .HasColumnType("integer");

                    b.Property<int?>("XPMultiplier")
                        .HasColumnType("integer");

                    b.Property<int?>("ZombieBMMove")
                        .HasColumnType("integer");

                    b.Property<int?>("ZombieFeralMove")
                        .HasColumnType("integer");

                    b.Property<int?>("ZombieFeralSense")
                        .HasColumnType("integer");

                    b.Property<int?>("ZombieMove")
                        .HasColumnType("integer");

                    b.Property<int?>("ZombieMoveNight")
                        .HasColumnType("integer");

                    b.Property<int?>("ZombiesRun")
                        .HasColumnType("integer");

                    b.HasIndex("DayCount");

                    b.HasIndex("EACEnabled");

                    b.HasIndex("IsPasswordProtected");

                    b.HasIndex("RequiresMod");

                    b.HasIndex("Version");

                    b.ToTable("SevenDTD_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.ARK.ArkServer", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<bool?>("Battleye")
                        .HasColumnType("boolean");

                    b.Property<string>("ClusterID")
                        .HasColumnType("text");

                    b.Property<string>("CustomServerName")
                        .HasColumnType("text");

                    b.Property<int?>("DaysRunning")
                        .HasColumnType("integer");

                    b.Property<bool?>("DownloadCharacters")
                        .HasColumnType("boolean");

                    b.Property<bool?>("DownloadItems")
                        .HasColumnType("boolean");

                    b.Property<bool?>("Modded")
                        .HasColumnType("boolean");

                    b.Property<List<string>>("Mods")
                        .HasColumnType("text[]");

                    b.Property<bool?>("OfficialServer")
                        .HasColumnType("boolean");

                    b.Property<bool?>("PVE")
                        .HasColumnType("boolean");

                    b.Property<bool?>("PasswordRequired")
                        .HasColumnType("boolean");

                    b.Property<int?>("SessionFlags")
                        .HasColumnType("integer");

                    b.HasIndex("Battleye");

                    b.HasIndex("PVE");

                    b.HasIndex("PasswordRequired");

                    b.ToTable("Ark_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.Arma3.Arma3Server", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<List<string>>("ModSignatures")
                        .HasColumnType("text[]");

                    b.ToTable("Arma3_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.DayZ.DayZServer", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<bool?>("AllowedBuild")
                        .HasColumnType("boolean");

                    b.Property<string>("Island")
                        .HasColumnType("text");

                    b.Property<string>("Language")
                        .HasColumnType("text");

                    b.Property<int?>("RequiredBuild")
                        .HasColumnType("integer");

                    b.Property<int?>("RequiredVersion")
                        .HasColumnType("integer");

                    b.Property<int?>("TimeLeft")
                        .HasColumnType("integer");

                    b.HasIndex("AllowedBuild");

                    b.HasIndex("Island");

                    b.ToTable("DayZ_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.HellLetLoose.HellLetLooseServer", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<int?>("SessionFlags")
                        .HasColumnType("integer");

                    b.Property<int?>("Visible")
                        .HasColumnType("integer");

                    b.HasIndex("Visible");

                    b.ToTable("HellLetLoose_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.PostScriptum.PostScriptumServer", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<bool?>("AllModsWhitelisted")
                        .HasColumnType("boolean");

                    b.Property<int?>("CurrentModLoadedCount")
                        .HasColumnType("integer");

                    b.Property<int?>("Flags")
                        .HasColumnType("integer");

                    b.Property<string>("GameMode")
                        .HasColumnType("text");

                    b.Property<string>("GameVersion")
                        .HasColumnType("text");

                    b.Property<int?>("MatchTimeout")
                        .HasColumnType("integer");

                    b.Property<bool?>("Password")
                        .HasColumnType("boolean");

                    b.Property<int?>("PlayerCount")
                        .HasColumnType("integer");

                    b.Property<int?>("PlayerReserveCount")
                        .HasColumnType("integer");

                    b.Property<int?>("PublicQueue")
                        .HasColumnType("integer");

                    b.Property<int?>("ReservedQueue")
                        .HasColumnType("integer");

                    b.Property<string>("SearchKeywords")
                        .HasColumnType("text");

                    b.Property<int?>("SessionFlags")
                        .HasColumnType("integer");

                    b.HasIndex("CurrentModLoadedCount");

                    b.HasIndex("GameMode");

                    b.ToTable("PostScriptum_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.ProjectZomboid.ProjectZomboidServer", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int?>("ModCount")
                        .HasColumnType("integer");

                    b.Property<string>("Mods")
                        .HasColumnType("text");

                    b.Property<bool?>("Open")
                        .HasColumnType("boolean");

                    b.Property<bool?>("Public")
                        .HasColumnType("boolean");

                    b.Property<bool?>("PvP")
                        .HasColumnType("boolean");

                    b.Property<string>("Version")
                        .HasColumnType("text");

                    b.HasIndex("Open");

                    b.HasIndex("PvP");

                    b.ToTable("ProjectZomboid_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.Rust.RustServer", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<int?>("AverageFPS")
                        .HasColumnType("integer");

                    b.Property<decimal?>("Build")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int?>("EntityCount")
                        .HasColumnType("integer");

                    b.Property<int?>("FPS")
                        .HasColumnType("integer");

                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<string>("HeaderImage")
                        .HasColumnType("text");

                    b.Property<string>("LogoImage")
                        .HasColumnType("text");

                    b.Property<bool?>("PvE")
                        .HasColumnType("boolean");

                    b.Property<string>("URL")
                        .HasColumnType("text");

                    b.Property<int?>("Uptime")
                        .HasColumnType("integer");

                    b.Property<int?>("WorldSeed")
                        .HasColumnType("integer");

                    b.Property<int?>("WorldSize")
                        .HasColumnType("integer");

                    b.Property<int?>("gc_cl")
                        .HasColumnType("integer");

                    b.Property<int?>("gc_mb")
                        .HasColumnType("integer");

                    b.HasIndex("EntityCount");

                    b.HasIndex("PvE");

                    b.ToTable("Rust_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.Unturned.UnturnedServer", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<string>("BrowserDescription")
                        .HasColumnType("text");

                    b.Property<string>("BrowserDescriptionHint")
                        .HasColumnType("text");

                    b.Property<string>("BrowserIcon")
                        .HasColumnType("text");

                    b.Property<List<string>>("CustomLinks")
                        .HasColumnType("text[]");

                    b.Property<string>("GameVersion")
                        .HasColumnType("text");

                    b.Property<string>("Mods")
                        .HasColumnType("text");

                    b.Property<string>("RocketPlugins")
                        .HasColumnType("text");

                    b.HasIndex("Mods");

                    b.ToTable("Unturned_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.VRising.VRisingServer", b =>
                {
                    b.HasBaseType("UncoreMetrics.Data.Server");

                    b.Property<bool?>("BloodBoundEquipment")
                        .HasColumnType("boolean");

                    b.Property<int?>("DaysRunning")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int?>("HeartDamage")
                        .HasColumnType("integer");

                    b.HasIndex("BloodBoundEquipment");

                    b.HasIndex("HeartDamage");

                    b.ToTable("V_Rising_Servers", (string)null);
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData._7DaysToDie.SevenDaysToDieServer", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData._7DaysToDie.SevenDaysToDieServer", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.ARK.ArkServer", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData.ARK.ArkServer", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.Arma3.Arma3Server", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData.Arma3.Arma3Server", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.DayZ.DayZServer", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData.DayZ.DayZServer", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.HellLetLoose.HellLetLooseServer", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData.HellLetLoose.HellLetLooseServer", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.PostScriptum.PostScriptumServer", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData.PostScriptum.PostScriptumServer", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.ProjectZomboid.ProjectZomboidServer", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData.ProjectZomboid.ProjectZomboidServer", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.Rust.RustServer", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData.Rust.RustServer", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.Unturned.UnturnedServer", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData.Unturned.UnturnedServer", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UncoreMetrics.Data.GameData.VRising.VRisingServer", b =>
                {
                    b.HasOne("UncoreMetrics.Data.Server", null)
                        .WithOne()
                        .HasForeignKey("UncoreMetrics.Data.GameData.VRising.VRisingServer", "ServerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
