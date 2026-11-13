using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryProfileJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileJson",
                table: "CountryContents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileJson",
                table: "CountryContents");
        }
    }
}
