using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UncoreMetrics.Data.Migrations.ServerContext
{
    public partial class ServerSearchIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Servers_Address",
                table: "Servers",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_Players",
                table: "Servers",
                column: "Players");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Servers_Address",
                table: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_Servers_Players",
                table: "Servers");
        }
    }
}
