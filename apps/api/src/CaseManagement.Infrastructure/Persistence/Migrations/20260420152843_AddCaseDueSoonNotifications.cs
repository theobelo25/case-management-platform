using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseDueSoonNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "case_due_soon_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlaDueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    WindowMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_case_due_soon_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_case_due_soon_notifications_cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_case_due_soon_notifications_users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_case_due_soon_notifications_CaseId_SlaDueAtUtc_WindowMinute~",
                table: "case_due_soon_notifications",
                columns: new[] { "CaseId", "SlaDueAtUtc", "WindowMinutes", "RecipientUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_case_due_soon_notifications_RecipientUserId",
                table: "case_due_soon_notifications",
                column: "RecipientUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "case_due_soon_notifications");
        }
    }
}
