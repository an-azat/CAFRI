using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicationImportWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "PublicationContentItems",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ImportedAtUtc",
                table: "PublicationContentItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresReview",
                table: "PublicationContentItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SourceDomain",
                table: "PublicationContentItems",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceName",
                table: "PublicationContentItems",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SourcePublishedAtUtc",
                table: "PublicationContentItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "PublicationContentItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowStatus",
                table: "PublicationContentItems",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.Sql("""
                UPDATE "PublicationContentItems"
                SET "WorkflowStatus" = CASE
                    WHEN "IsPublished" = TRUE THEN 'Published'
                    ELSE 'Draft'
                END
                WHERE "WorkflowStatus" = 'Draft' OR "WorkflowStatus" = '';
                """);

            migrationBuilder.CreateTable(
                name: "PublicationCategoryAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    CategorySlug = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationCategoryAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationCategoryAssignments_PublicationContentItems_Publ~",
                        column: x => x.PublicationContentItemId,
                        principalTable: "PublicationContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationCountryAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CountryLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationCountryAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationCountryAssignments_PublicationContentItems_Publi~",
                        column: x => x.PublicationContentItemId,
                        principalTable: "PublicationContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationContentItems_ExternalId",
                table: "PublicationContentItems",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationContentItems_RequiresReview",
                table: "PublicationContentItems",
                column: "RequiresReview");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationContentItems_SourceDomain",
                table: "PublicationContentItems",
                column: "SourceDomain");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationContentItems_SourcePublishedAtUtc",
                table: "PublicationContentItems",
                column: "SourcePublishedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationContentItems_SourceUrl",
                table: "PublicationContentItems",
                column: "SourceUrl");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationContentItems_WorkflowStatus",
                table: "PublicationContentItems",
                column: "WorkflowStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationCategoryAssignments_PublicationContentItemId_Cat~",
                table: "PublicationCategoryAssignments",
                columns: new[] { "PublicationContentItemId", "CategorySlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicationCountryAssignments_PublicationContentItemId_Coun~",
                table: "PublicationCountryAssignments",
                columns: new[] { "PublicationContentItemId", "CountryCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicationCategoryAssignments");

            migrationBuilder.DropTable(
                name: "PublicationCountryAssignments");

            migrationBuilder.DropIndex(
                name: "IX_PublicationContentItems_ExternalId",
                table: "PublicationContentItems");

            migrationBuilder.DropIndex(
                name: "IX_PublicationContentItems_RequiresReview",
                table: "PublicationContentItems");

            migrationBuilder.DropIndex(
                name: "IX_PublicationContentItems_SourceDomain",
                table: "PublicationContentItems");

            migrationBuilder.DropIndex(
                name: "IX_PublicationContentItems_SourcePublishedAtUtc",
                table: "PublicationContentItems");

            migrationBuilder.DropIndex(
                name: "IX_PublicationContentItems_SourceUrl",
                table: "PublicationContentItems");

            migrationBuilder.DropIndex(
                name: "IX_PublicationContentItems_WorkflowStatus",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "ImportedAtUtc",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "RequiresReview",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "SourceDomain",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "SourceName",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "SourcePublishedAtUtc",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "WorkflowStatus",
                table: "PublicationContentItems");
        }
    }
}
