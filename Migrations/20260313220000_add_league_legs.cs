using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace competitions.Migrations
{
    /// <inheritdoc />
    public partial class add_league_legs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Legs",
                table: "LeagueConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Legs",
                table: "LeagueConfigs");
        }
    }
}
