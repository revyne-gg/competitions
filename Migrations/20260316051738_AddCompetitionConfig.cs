using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace competitions.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitionConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BracketReset",
                table: "TournamentConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SeedingType",
                table: "TournamentConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BestOf",
                table: "Competitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Game",
                table: "Competitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "MapPool",
                table: "Competitions",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BracketReset",
                table: "TournamentConfigs");

            migrationBuilder.DropColumn(
                name: "SeedingType",
                table: "TournamentConfigs");

            migrationBuilder.DropColumn(
                name: "BestOf",
                table: "Competitions");

            migrationBuilder.DropColumn(
                name: "Game",
                table: "Competitions");

            migrationBuilder.DropColumn(
                name: "MapPool",
                table: "Competitions");
        }
    }
}
