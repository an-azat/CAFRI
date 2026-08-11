using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHomePageContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomePageContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HeroTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    HeroLead = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    HeroPrimaryCtaLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    HeroPrimaryCtaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HeroSecondaryCtaLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    HeroSecondaryCtaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IntroTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IntroDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PlatformSectionLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PlatformSectionTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PlatformSectionDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DirectionsText = table.Column<string>(type: "text", nullable: false),
                    CountriesSectionLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CountriesSectionTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CountriesSectionDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FeaturesSectionLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FeaturesSectionTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FeaturesSectionDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FeatureCardsText = table.Column<string>(type: "text", nullable: false),
                    LatestSectionLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    LatestSectionTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    LatestSectionDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FeaturedPublicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AboutTitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    AboutDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CoverageAreasText = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePageContents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomePageContents_FeaturedPublicationId",
                table: "HomePageContents",
                column: "FeaturedPublicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomePageContents");
        }
    }
}
