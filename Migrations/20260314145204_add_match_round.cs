using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace competitions.Migrations
{
    /// <inheritdoc />
    public partial class add_match_round : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "LeagueTeams",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRegistrationOpen",
                table: "LeagueConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaguePeriodEnd",
                table: "LeagueConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaguePeriodStart",
                table: "LeagueConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationPeriodEnd",
                table: "LeagueConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationPeriodStart",
                table: "LeagueConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "LeagueConfigs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Divisions",
                type: "text",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "BestOf",
                table: "Divisions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CompetitionId",
                table: "Divisions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Divisions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaxTeamsPerGroup",
                table: "Divisions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Divisions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Divisions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Divisions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "DivisionGroups",
                type: "text",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DivisionGroups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DivisionId",
                table: "DivisionGroups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "DivisionGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "DivisionGroups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "DivisionGroups",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DivisionGroupTeams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DivisionGroupTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DivisionGroupTeams_DivisionGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "DivisionGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompetitionId = table.Column<string>(type: "text", nullable: false),
                    HomeTeamId = table.Column<string>(type: "text", nullable: true),
                    AwayTeamId = table.Column<string>(type: "text", nullable: true),
                    ScoreHome = table.Column<int>(type: "integer", nullable: true),
                    ScoreAway = table.Column<int>(type: "integer", nullable: true),
                    WinnerTeamId = table.Column<string>(type: "text", nullable: true),
                    LoserTeamId = table.Column<string>(type: "text", nullable: true),
                    Round = table.Column<int>(type: "integer", nullable: true),
                    MatchDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_CompetitionId",
                table: "Divisions",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionGroups_DivisionId",
                table: "DivisionGroups",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_DivisionGroupTeams_GroupId",
                table: "DivisionGroupTeams",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CompetitionId",
                table: "Matches",
                column: "CompetitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DivisionGroups_Divisions_DivisionId",
                table: "DivisionGroups",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Divisions_Competitions_CompetitionId",
                table: "Divisions",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DivisionGroups_Divisions_DivisionId",
                table: "DivisionGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Divisions_Competitions_CompetitionId",
                table: "Divisions");

            migrationBuilder.DropTable(
                name: "DivisionGroupTeams");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Divisions_CompetitionId",
                table: "Divisions");

            migrationBuilder.DropIndex(
                name: "IX_DivisionGroups_DivisionId",
                table: "DivisionGroups");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LeagueTeams");

            migrationBuilder.DropColumn(
                name: "IsRegistrationOpen",
                table: "LeagueConfigs");

            migrationBuilder.DropColumn(
                name: "LeaguePeriodEnd",
                table: "LeagueConfigs");

            migrationBuilder.DropColumn(
                name: "LeaguePeriodStart",
                table: "LeagueConfigs");

            migrationBuilder.DropColumn(
                name: "RegistrationPeriodEnd",
                table: "LeagueConfigs");

            migrationBuilder.DropColumn(
                name: "RegistrationPeriodStart",
                table: "LeagueConfigs");

            migrationBuilder.DropColumn(
                name: "State",
                table: "LeagueConfigs");

            migrationBuilder.DropColumn(
                name: "BestOf",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "MaxTeamsPerGroup",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DivisionGroups");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "DivisionGroups");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "DivisionGroups");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "DivisionGroups");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DivisionGroups");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Divisions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "DivisionGroups",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
