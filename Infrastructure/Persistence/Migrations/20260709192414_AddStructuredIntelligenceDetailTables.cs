using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredIntelligenceDetailTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntelligenceArticleSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntelligenceContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Heading = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceArticleSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceArticleSections_IntelligenceContentItems_Intell~",
                        column: x => x.IntelligenceContentItemId,
                        principalTable: "IntelligenceContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceDocumentInfoEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntelligenceContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceDocumentInfoEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceDocumentInfoEntries_IntelligenceContentItems_In~",
                        column: x => x.IntelligenceContentItemId,
                        principalTable: "IntelligenceContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceHighlightEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntelligenceContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceHighlightEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceHighlightEntries_IntelligenceContentItems_Intel~",
                        column: x => x.IntelligenceContentItemId,
                        principalTable: "IntelligenceContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceImpactEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntelligenceContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImpactLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ImpactTone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceImpactEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceImpactEntries_IntelligenceContentItems_Intellig~",
                        column: x => x.IntelligenceContentItemId,
                        principalTable: "IntelligenceContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceKeyChangeEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntelligenceContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceKeyChangeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceKeyChangeEntries_IntelligenceContentItems_Intel~",
                        column: x => x.IntelligenceContentItemId,
                        principalTable: "IntelligenceContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceOfficialDocumentEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntelligenceContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Meta = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    DownloadUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceOfficialDocumentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceOfficialDocumentEntries_IntelligenceContentItem~",
                        column: x => x.IntelligenceContentItemId,
                        principalTable: "IntelligenceContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceRelatedLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntelligenceContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceRelatedLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceRelatedLinks_IntelligenceContentItems_Intellige~",
                        column: x => x.IntelligenceContentItemId,
                        principalTable: "IntelligenceContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceStatusEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntelligenceContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Value = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsStatus = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceStatusEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceStatusEntries_IntelligenceContentItems_Intellig~",
                        column: x => x.IntelligenceContentItemId,
                        principalTable: "IntelligenceContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntelligenceTimelineEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntelligenceContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Stage = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntelligenceTimelineEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceTimelineEntries_IntelligenceContentItems_Intell~",
                        column: x => x.IntelligenceContentItemId,
                        principalTable: "IntelligenceContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceArticleSections_IntelligenceContentItemId_Secti~",
                table: "IntelligenceArticleSections",
                columns: new[] { "IntelligenceContentItemId", "SectionKey", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceDocumentInfoEntries_IntelligenceContentItemId_D~",
                table: "IntelligenceDocumentInfoEntries",
                columns: new[] { "IntelligenceContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceHighlightEntries_IntelligenceContentItemId_Disp~",
                table: "IntelligenceHighlightEntries",
                columns: new[] { "IntelligenceContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceImpactEntries_IntelligenceContentItemId_Display~",
                table: "IntelligenceImpactEntries",
                columns: new[] { "IntelligenceContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceKeyChangeEntries_IntelligenceContentItemId_Disp~",
                table: "IntelligenceKeyChangeEntries",
                columns: new[] { "IntelligenceContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceOfficialDocumentEntries_IntelligenceContentItem~",
                table: "IntelligenceOfficialDocumentEntries",
                columns: new[] { "IntelligenceContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceRelatedLinks_IntelligenceContentItemId_LinkType~",
                table: "IntelligenceRelatedLinks",
                columns: new[] { "IntelligenceContentItemId", "LinkType", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceStatusEntries_IntelligenceContentItemId_Display~",
                table: "IntelligenceStatusEntries",
                columns: new[] { "IntelligenceContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceTimelineEntries_IntelligenceContentItemId_Displ~",
                table: "IntelligenceTimelineEntries",
                columns: new[] { "IntelligenceContentItemId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntelligenceArticleSections");

            migrationBuilder.DropTable(
                name: "IntelligenceDocumentInfoEntries");

            migrationBuilder.DropTable(
                name: "IntelligenceHighlightEntries");

            migrationBuilder.DropTable(
                name: "IntelligenceImpactEntries");

            migrationBuilder.DropTable(
                name: "IntelligenceKeyChangeEntries");

            migrationBuilder.DropTable(
                name: "IntelligenceOfficialDocumentEntries");

            migrationBuilder.DropTable(
                name: "IntelligenceRelatedLinks");

            migrationBuilder.DropTable(
                name: "IntelligenceStatusEntries");

            migrationBuilder.DropTable(
                name: "IntelligenceTimelineEntries");
        }
    }
}
