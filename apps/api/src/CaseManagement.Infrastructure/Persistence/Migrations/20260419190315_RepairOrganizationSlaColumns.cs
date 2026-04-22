using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RepairOrganizationSlaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent repair: migration history can show AddOrganizationSlaPolicyHours as applied
            // while columns are missing (manual DB edits, restore from backup, wrong DB, etc.).
            migrationBuilder.Sql(
                """
                ALTER TABLE organizations ADD COLUMN IF NOT EXISTS "SlaLowHours" integer NOT NULL DEFAULT 24;
                ALTER TABLE organizations ADD COLUMN IF NOT EXISTS "SlaMediumHours" integer NOT NULL DEFAULT 8;
                ALTER TABLE organizations ADD COLUMN IF NOT EXISTS "SlaHighHours" integer NOT NULL DEFAULT 4;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Repair migration is not reversed: dropping columns could destroy data.
        }
    }
}
