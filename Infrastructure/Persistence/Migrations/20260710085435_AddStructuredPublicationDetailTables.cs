using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredPublicationDetailTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorLabel",
                table: "PublicationContentItems",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentLabel",
                table: "PublicationContentItems",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HeroVisualClassName",
                table: "PublicationContentItems",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PdfDownloadUrl",
                table: "PublicationContentItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedDate",
                table: "PublicationContentItems",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReadingTime",
                table: "PublicationContentItems",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "PublicationContentItems",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PublicationActionEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationActionEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationActionEntries_PublicationContentItems_Publicatio~",
                        column: x => x.PublicationContentItemId,
                        principalTable: "PublicationContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationArticleSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Heading = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParagraphsText = table.Column<string>(type: "text", nullable: false),
                    BulletPointsText = table.Column<string>(type: "text", nullable: false),
                    CalloutTone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CalloutLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CalloutTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CalloutDescription = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationArticleSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationArticleSections_PublicationContentItems_Publicat~",
                        column: x => x.PublicationContentItemId,
                        principalTable: "PublicationContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationDocumentEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Meta = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DownloadUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationDocumentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationDocumentEntries_PublicationContentItems_Publicat~",
                        column: x => x.PublicationContentItemId,
                        principalTable: "PublicationContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationFindingEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Icon = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationFindingEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationFindingEntries_PublicationContentItems_Publicati~",
                        column: x => x.PublicationContentItemId,
                        principalTable: "PublicationContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationHighlightEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationHighlightEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationHighlightEntries_PublicationContentItems_Publica~",
                        column: x => x.PublicationContentItemId,
                        principalTable: "PublicationContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationInfoEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationInfoEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationInfoEntries_PublicationContentItems_PublicationC~",
                        column: x => x.PublicationContentItemId,
                        principalTable: "PublicationContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicationRelatedLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicationContentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Meta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicationRelatedLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicationRelatedLinks_PublicationContentItems_Publication~",
                        column: x => x.PublicationContentItemId,
                        principalTable: "PublicationContentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationActionEntries_PublicationContentItemId_DisplayOr~",
                table: "PublicationActionEntries",
                columns: new[] { "PublicationContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationArticleSections_PublicationContentItemId_Section~",
                table: "PublicationArticleSections",
                columns: new[] { "PublicationContentItemId", "SectionKey", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationDocumentEntries_PublicationContentItemId_Display~",
                table: "PublicationDocumentEntries",
                columns: new[] { "PublicationContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationFindingEntries_PublicationContentItemId_DisplayO~",
                table: "PublicationFindingEntries",
                columns: new[] { "PublicationContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationHighlightEntries_PublicationContentItemId_Displa~",
                table: "PublicationHighlightEntries",
                columns: new[] { "PublicationContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationInfoEntries_PublicationContentItemId_DisplayOrder",
                table: "PublicationInfoEntries",
                columns: new[] { "PublicationContentItemId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationRelatedLinks_PublicationContentItemId_LinkType_D~",
                table: "PublicationRelatedLinks",
                columns: new[] { "PublicationContentItemId", "LinkType", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicationActionEntries");

            migrationBuilder.DropTable(
                name: "PublicationArticleSections");

            migrationBuilder.DropTable(
                name: "PublicationDocumentEntries");

            migrationBuilder.DropTable(
                name: "PublicationFindingEntries");

            migrationBuilder.DropTable(
                name: "PublicationHighlightEntries");

            migrationBuilder.DropTable(
                name: "PublicationInfoEntries");

            migrationBuilder.DropTable(
                name: "PublicationRelatedLinks");

            migrationBuilder.DropColumn(
                name: "AuthorLabel",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "DocumentLabel",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "HeroVisualClassName",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "PdfDownloadUrl",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "ReadingTime",
                table: "PublicationContentItems");

            migrationBuilder.DropColumn(
                name: "Topic",
                table: "PublicationContentItems");
        }
    }
}
