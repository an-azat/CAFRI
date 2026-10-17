using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIntelligenceTrending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTrending",
                table: "IntelligenceContentItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TrendingOrder",
                table: "IntelligenceContentItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceContentItems_IsTrending_TrendingOrder",
                table: "IntelligenceContentItems",
                columns: new[] { "IsTrending", "TrendingOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IntelligenceContentItems_IsTrending_TrendingOrder",
                table: "IntelligenceContentItems");

            migrationBuilder.DropColumn(
                name: "IsTrending",
                table: "IntelligenceContentItems");

            migrationBuilder.DropColumn(
                name: "TrendingOrder",
                table: "IntelligenceContentItems");
        }
    }
}
