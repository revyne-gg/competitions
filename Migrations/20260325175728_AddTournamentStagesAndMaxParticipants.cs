using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace competitions.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentStagesAndMaxParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "TournamentConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TournamentStages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompetitionId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Advancing = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    FormatConfigJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentStages_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentStages_CompetitionId",
                table: "TournamentStages",
                column: "CompetitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentStages");

            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "TournamentConfigs");
        }
    }
}
