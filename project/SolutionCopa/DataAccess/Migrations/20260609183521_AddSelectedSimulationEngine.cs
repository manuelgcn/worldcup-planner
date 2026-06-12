using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedSimulationEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedSimulationEngine",
                table: "TournamentSettings",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "Probabilístico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedSimulationEngine",
                table: "TournamentSettings");
        }
    }
}
