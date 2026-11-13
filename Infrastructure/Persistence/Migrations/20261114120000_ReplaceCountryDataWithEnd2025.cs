using System.Text.Json;
using CAFRI.Infrastructure.Content;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Data-only migration: replaces all country profile data and the CountryIndicators rows of
    /// the five covered countries with end-2025 data (see CountryReferenceData2025 for sources).
    /// Photos, flags, slugs, names, publication flags and display order are kept.
    /// The previous values are not restored by Down().
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261114120000_ReplaceCountryDataWithEnd2025")]
    public partial class ReplaceCountryDataWithEnd2025 : Migration
    {
        private static readonly string[] CountryCodes = ["KZ", "UZ", "KG", "TJ", "TM"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var now = new DateTimeOffset(2026, 11, 14, 0, 0, 0, TimeSpan.Zero);

            migrationBuilder.Sql("""DELETE FROM "CountryIndicators" WHERE "CountryCode" IN ('KZ', 'UZ', 'KG', 'TJ', 'TM');""");

            var rows = CountryReferenceData2025.BuildIndicators();
            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                // Column types are given explicitly because this migration has no target model.
                migrationBuilder.InsertData(
                    table: "CountryIndicators",
                    columns: ["Id", "CountryCode", "Name", "Slug", "Category", "Unit", "LatestValue", "YearLabel", "SourceName", "Notes", "DisplayOrder", "IsPublished", "CreatedAtUtc", "UpdatedAtUtc"],
                    columnTypes: ["uuid", "character varying(10)", "character varying(180)", "character varying(180)", "character varying(120)", "character varying(40)", "character varying(120)", "character varying(60)", "character varying(220)", "character varying(1000)", "integer", "boolean", "timestamp with time zone", "timestamp with time zone"],
                    values: new object[] { Guid.NewGuid(), row.CountryCode, row.Name, row.Slug, row.Category, row.Unit, row.LatestValue, row.YearLabel, row.SourceName, row.Notes, index, true, now, now });
            }

            foreach (var country in CountryReferenceData2025.Countries.Where(x => CountryCodes.Contains(x.Code)))
            {
                migrationBuilder.UpdateData(
                    table: "CountryContents",
                    keyColumns: ["Code"],
                    keyColumnTypes: ["character varying(10)"],
                    keyValues: [country.Code],
                    columns: ["OfficialName", "Region", "Summary", "ProfileLead", "AboutText", "Capital", "Currency", "OfficialLanguages", "Area", "NeighbouringCountries", "Population", "PopulationPeriod", "Gdp", "GdpPeriod", "DataSource", "BankAssets", "ProfileJson", "DetailsJson", "UpdatedAtUtc"],
                    columnTypes: ["character varying(150)", "character varying(100)", "character varying(1000)", "character varying(600)", "text", "character varying(150)", "character varying(100)", "character varying(150)", "character varying(100)", "character varying(200)", "character varying(100)", "character varying(150)", "character varying(100)", "character varying(150)", "character varying(200)", "character varying(100)", "text", "text", "timestamp with time zone"],
                    values: new object[]
                    {
                        country.OfficialName, "Central Asia", country.Summary, country.ProfileLead, country.AboutText, country.Capital, country.Currency,
                        country.OfficialLanguages, country.Area, country.NeighbouringCountries, country.Population, CountryReferenceData2025.PopulationPeriod,
                        country.Gdp, CountryReferenceData2025.GdpPeriod, CountryReferenceData2025.DataSource, string.Empty,
                        JsonSerializer.Serialize(CountryReferenceData2025.BuildProfile(country.Code)), string.Empty, now
                    });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data replacement only; the earlier illustrative values are intentionally not restored.
        }
    }
}
