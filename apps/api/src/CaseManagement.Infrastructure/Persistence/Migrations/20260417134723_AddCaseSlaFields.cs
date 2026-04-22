using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseSlaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SlaBreachedAtUtc",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SlaDueAtUtc",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SlaPausedAtUtc",
                table: "cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlaRemainingSeconds",
                table: "cases",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlaBreachedAtUtc",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "SlaDueAtUtc",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "SlaPausedAtUtc",
                table: "cases");

            migrationBuilder.DropColumn(
                name: "SlaRemainingSeconds",
                table: "cases");
        }
    }
}
