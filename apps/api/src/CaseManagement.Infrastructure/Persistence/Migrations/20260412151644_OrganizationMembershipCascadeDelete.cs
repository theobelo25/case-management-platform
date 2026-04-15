using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CaseManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationMembershipCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_organization_memberships_organizations_OrganizationId",
                table: "organization_memberships");

            migrationBuilder.AddForeignKey(
                name: "FK_organization_memberships_organizations_OrganizationId",
                table: "organization_memberships",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_organization_memberships_organizations_OrganizationId",
                table: "organization_memberships");

            migrationBuilder.AddForeignKey(
                name: "FK_organization_memberships_organizations_OrganizationId",
                table: "organization_memberships",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
