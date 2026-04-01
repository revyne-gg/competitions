using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace competitions.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationTypeToTournamentConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationPassword",
                table: "TournamentConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegistrationType",
                table: "TournamentConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationPassword",
                table: "TournamentConfigs");

            migrationBuilder.DropColumn(
                name: "RegistrationType",
                table: "TournamentConfigs");
        }
    }
}
