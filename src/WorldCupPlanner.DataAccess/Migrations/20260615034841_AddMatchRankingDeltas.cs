using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldCupPlanner.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchRankingDeltas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppliedAwayRankingDelta",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AppliedHomeRankingDelta",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedAwayRankingDelta",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "AppliedHomeRankingDelta",
                table: "Matches");
        }
    }
}
