using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshSessionClientAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientIpAddress",
                table: "refresh_sessions",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientUserAgent",
                table: "refresh_sessions",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientIpAddress",
                table: "refresh_sessions");

            migrationBuilder.DropColumn(
                name: "ClientUserAgent",
                table: "refresh_sessions");
        }
    }
}
