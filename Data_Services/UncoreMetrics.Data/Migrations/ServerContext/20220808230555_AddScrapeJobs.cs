using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UncoreMetrics.Data.Migrations.ServerContext
{
    public partial class AddScrapeJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Scrape_Jobs",
                columns: table => new
                {
                    InternalId = table.Column<string>(type: "text", nullable: false),
                    GameType = table.Column<string>(type: "text", nullable: false),
                    RunType = table.Column<string>(type: "text", nullable: false),
                    Node = table.Column<string>(type: "text", nullable: false),
                    RunId = table.Column<int>(type: "integer", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    TotalDone = table.Column<int>(type: "integer", nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    RunGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Running = table.Column<bool>(type: "boolean", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scrape_Jobs", x => x.InternalId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Scrape_Jobs");
        }
    }
}
