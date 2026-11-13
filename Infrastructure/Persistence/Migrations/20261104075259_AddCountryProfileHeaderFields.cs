using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryProfileHeaderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AboutText",
                table: "CountryContents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "CountryContents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "CountryContents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataSource",
                table: "CountryContents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FlagImageUrl",
                table: "CountryContents",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GdpPeriod",
                table: "CountryContents",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NeighbouringCountries",
                table: "CountryContents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OfficialLanguages",
                table: "CountryContents",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OfficialName",
                table: "CountryContents",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PopulationPeriod",
                table: "CountryContents",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileLead",
                table: "CountryContents",
                type: "character varying(600)",
                maxLength: 600,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "CountryContents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Currency and the profile lead used to live only inside DetailsJson; copy them out.
            // A row whose DetailsJson is not valid JSON simply keeps the empty default.
            migrationBuilder.Sql("""
                CREATE FUNCTION pg_temp.cafri_json_field(raw text, field text) RETURNS text AS $$
                BEGIN
                    RETURN coalesce(raw::jsonb ->> field, '');
                EXCEPTION WHEN others THEN
                    RETURN '';
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql("""
                UPDATE "CountryContents"
                SET "Currency" = left(pg_temp.cafri_json_field("DetailsJson", 'Currency'), 100),
                    "ProfileLead" = left(pg_temp.cafri_json_field("DetailsJson", 'HeroDescription'), 600);
                """);

            migrationBuilder.Sql("DROP FUNCTION pg_temp.cafri_json_field(text, text);");

            // Stable reference facts for the five covered countries. Area, neighbours and the
            // population/GDP periods are left for the editors to fill in with sourced values.
            migrationBuilder.Sql("""
                UPDATE "CountryContents" SET "Region" = 'Central Asia';
                UPDATE "CountryContents" SET "OfficialName" = 'Republic of Kazakhstan', "OfficialLanguages" = 'Kazakh · Russian' WHERE "Code" = 'KZ';
                UPDATE "CountryContents" SET "OfficialName" = 'Republic of Uzbekistan', "OfficialLanguages" = 'Uzbek' WHERE "Code" = 'UZ';
                UPDATE "CountryContents" SET "OfficialName" = 'Kyrgyz Republic', "OfficialLanguages" = 'Kyrgyz · Russian' WHERE "Code" = 'KG';
                UPDATE "CountryContents" SET "OfficialName" = 'Republic of Tajikistan', "OfficialLanguages" = 'Tajik' WHERE "Code" = 'TJ';
                UPDATE "CountryContents" SET "OfficialName" = 'Turkmenistan', "OfficialLanguages" = 'Turkmen' WHERE "Code" = 'TM';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AboutText",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "DataSource",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "FlagImageUrl",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "GdpPeriod",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "NeighbouringCountries",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "OfficialLanguages",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "OfficialName",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "PopulationPeriod",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "ProfileLead",
                table: "CountryContents");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "CountryContents");
        }
    }
}
