using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Replaces the free-text Intelligence PublishedLabel ("Jun 15, 2026") with a real timestamp,
    /// so the archive date filter and date sorts use the publication date instead of the last edit.
    /// Labels that cannot be parsed fall back to CreatedAtUtc.
    /// </summary>
    public partial class ConvertIntelligenceDateToTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishedAtUtc",
                table: "IntelligenceContentItems",
                type: "timestamp with time zone",
                nullable: true);

            // Tries each known text format and returns NULL instead of failing on bad input.
            migrationBuilder.Sql("""
                CREATE FUNCTION pg_temp.cafri_parse_intelligence_date(raw text) RETURNS timestamptz AS $$
                DECLARE
                    value text := btrim(coalesce(raw, ''));
                BEGIN
                    IF value = '' THEN
                        RETURN NULL;
                    END IF;

                    BEGIN
                        IF value ~ '^\d{4}-\d{2}-\d{2}' THEN
                            RETURN (substr(value, 1, 10)::date)::timestamp AT TIME ZONE 'UTC';
                        ELSIF value ~ '^[A-Za-z]{3} \d{1,2}, \d{4}$' THEN
                            RETURN to_date(value, 'Mon DD, YYYY')::timestamp AT TIME ZONE 'UTC';
                        ELSIF value ~ '^[A-Za-z]+ \d{1,2}, \d{4}$' THEN
                            RETURN to_date(value, 'FMMonth DD, YYYY')::timestamp AT TIME ZONE 'UTC';
                        ELSIF value ~ '^\d{1,2} [A-Za-z]+ \d{4}$' THEN
                            RETURN to_date(value, 'DD FMMonth YYYY')::timestamp AT TIME ZONE 'UTC';
                        ELSIF value ~ '^[A-Za-z]{3} \d{4}$' THEN
                            RETURN to_date(value, 'Mon YYYY')::timestamp AT TIME ZONE 'UTC';
                        ELSIF value ~ '^[A-Za-z]+ \d{4}$' THEN
                            RETURN to_date(value, 'FMMonth YYYY')::timestamp AT TIME ZONE 'UTC';
                        END IF;
                    EXCEPTION WHEN others THEN
                        RETURN NULL;
                    END;

                    RETURN NULL;
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql("""
                UPDATE "IntelligenceContentItems"
                SET "PublishedAtUtc" = coalesce(pg_temp.cafri_parse_intelligence_date("PublishedLabel"), "CreatedAtUtc");
                """);

            migrationBuilder.Sql("DROP FUNCTION pg_temp.cafri_parse_intelligence_date(text);");

            migrationBuilder.DropColumn(
                name: "PublishedLabel",
                table: "IntelligenceContentItems");

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceContentItems_PublishedAtUtc",
                table: "IntelligenceContentItems",
                column: "PublishedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IntelligenceContentItems_PublishedAtUtc",
                table: "IntelligenceContentItems");

            migrationBuilder.AddColumn<string>(
                name: "PublishedLabel",
                table: "IntelligenceContentItems",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "IntelligenceContentItems"
                SET "PublishedLabel" = coalesce(to_char("PublishedAtUtc" AT TIME ZONE 'UTC', 'Mon FMDD, YYYY'), '');
                """);

            migrationBuilder.DropColumn(
                name: "PublishedAtUtc",
                table: "IntelligenceContentItems");
        }
    }
}
