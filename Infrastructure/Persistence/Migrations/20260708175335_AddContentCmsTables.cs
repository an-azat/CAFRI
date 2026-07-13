using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentCmsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountryContents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Capital = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Gdp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Population = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BankAssets = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HeroClassName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DetailsJson = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceContentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Category = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CountryName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PublishedLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DetailsJson = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceContentItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicationContentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CountryLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Meta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DetailsJson = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationContentItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountryContents_Code",
                table: "CountryContents",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountryContents_IsPublished_DisplayOrder",
                table: "CountryContents",
                columns: new[] { "IsPublished", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_CountryContents_Slug",
                table: "CountryContents",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceContentItems_IsPublished_DisplayOrder",
                table: "IntelligenceContentItems",
                columns: new[] { "IsPublished", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceContentItems_Slug",
                table: "IntelligenceContentItems",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicationContentItems_IsPublished_DisplayOrder",
                table: "PublicationContentItems",
                columns: new[] { "IsPublished", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationContentItems_Slug",
                table: "PublicationContentItems",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryContents");

            migrationBuilder.DropTable(
                name: "IntelligenceContentItems");

            migrationBuilder.DropTable(
                name: "PublicationContentItems");
        }
    }
}
