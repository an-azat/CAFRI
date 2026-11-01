using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicationVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "PublicationContentItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "SignedIn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "PublicationContentItems");
        }
    }
}
