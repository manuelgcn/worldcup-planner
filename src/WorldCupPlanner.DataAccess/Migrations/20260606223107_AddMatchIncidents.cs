using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldCupPlanner.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchIncidents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Minute = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    IncidentType = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    MatchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchIncidents_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchIncidents_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchIncidents_MatchId",
                table: "MatchIncidents",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchIncidents_TeamId",
                table: "MatchIncidents",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchIncidents");
        }
    }
}
