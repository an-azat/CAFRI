using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentMediaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GalleryJson",
                table: "PublicationContentItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroImageUrl",
                table: "PublicationContentItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroImageUrl",
                table: "IntelligenceContentItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroGalleryJson",
                table: "CountryContents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GalleryJson",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "HeroImageUrl",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "HeroImageUrl",
                table: "IntelligenceContentItems");

            migrationBuilder.DropColumn(
                name: "HeroGalleryJson",
                table: "CountryContents");
        }
    }
}
