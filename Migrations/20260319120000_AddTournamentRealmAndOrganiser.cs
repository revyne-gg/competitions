using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace competitions.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentRealmAndOrganiser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganiserId",
                table: "TournamentConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RealmId",
                table: "TournamentConfigs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganiserId",
                table: "TournamentConfigs");

            migrationBuilder.DropColumn(
                name: "RealmId",
                table: "TournamentConfigs");
        }
    }
}
