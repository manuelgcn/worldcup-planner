using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldCupPlanner.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStadiumsSettingsAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TournamentSettingsId = table.Column<int>(type: "int", nullable: false),
                    AffectedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AffectedEntityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stadiums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    City = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stadiums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TournamentSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxMatchesPerDay = table.Column<int>(type: "int", nullable: false),
                    DaysBetweenJourneys = table.Column<int>(type: "int", nullable: false),
                    FixtureSeed = table.Column<int>(type: "int", nullable: false),
                    CrossingsSeed = table.Column<int>(type: "int", nullable: false),
                    SimulationSeed = table.Column<int>(type: "int", nullable: false),
                    CompletionSeed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stadiums_Name",
                table: "Stadiums",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Stadiums");

            migrationBuilder.DropTable(
                name: "TournamentSettings");
        }
    }
}
