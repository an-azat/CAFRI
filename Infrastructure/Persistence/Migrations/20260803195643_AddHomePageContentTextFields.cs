using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHomePageContentTextFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountriesVisualDescription",
                table: "HomePageContents",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountriesVisualTitle",
                table: "HomePageContents",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FeaturedPublicationEyebrow",
                table: "HomePageContents",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LatestFeaturedLinkLabel",
                table: "HomePageContents",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LatestIntelligenceLinkLabel",
                table: "HomePageContents",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountriesVisualDescription",
                table: "HomePageContents");

            migrationBuilder.DropColumn(
                name: "CountriesVisualTitle",
                table: "HomePageContents");

            migrationBuilder.DropColumn(
                name: "FeaturedPublicationEyebrow",
                table: "HomePageContents");

            migrationBuilder.DropColumn(
                name: "LatestFeaturedLinkLabel",
                table: "HomePageContents");

            migrationBuilder.DropColumn(
                name: "LatestIntelligenceLinkLabel",
                table: "HomePageContents");
        }
    }
}
