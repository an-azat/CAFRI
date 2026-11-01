using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Replaces the free-text PublishedDate ("June 2026") with a real timestamp.
    /// Existing values are converted before the old column is dropped: PublishedDate first,
    /// then the date part of Meta ("June 2026 | 12 min read"), then SourcePublishedAtUtc.
    /// Month-only values become the 1st of that month (00:00 UTC).
    /// </summary>
    public partial class ConvertPublicationDateToTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishedAtUtc",
                table: "PublicationContentItems",
                type: "timestamp with time zone",
                nullable: true);

            // Tries each known text format and returns NULL instead of failing on bad input.
            migrationBuilder.Sql("""
                CREATE FUNCTION pg_temp.cafri_parse_publication_date(raw text) RETURNS timestamptz AS $$
                DECLARE
                    value text := btrim(coalesce(raw, ''));
                BEGIN
                    IF value = '' THEN
                        RETURN NULL;
                    END IF;

                    BEGIN
                        IF value ~ '^\d{4}-\d{2}-\d{2}' THEN
                            RETURN (substr(value, 1, 10)::date)::timestamp AT TIME ZONE 'UTC';
                        ELSIF value ~ '^\d{4}-\d{2}$' THEN
                            RETURN ((value || '-01')::date)::timestamp AT TIME ZONE 'UTC';
                        ELSIF value ~ '^\d{1,2} [A-Za-z]+ \d{4}$' THEN
                            RETURN to_date(value, 'DD FMMonth YYYY')::timestamp AT TIME ZONE 'UTC';
                        ELSIF value ~ '^[A-Za-z]+ \d{1,2}, \d{4}$' THEN
                            RETURN to_date(value, 'FMMonth DD, YYYY')::timestamp AT TIME ZONE 'UTC';
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
                UPDATE "PublicationContentItems"
                SET "PublishedAtUtc" = coalesce(
                    pg_temp.cafri_parse_publication_date("PublishedDate"),
                    pg_temp.cafri_parse_publication_date(split_part("Meta", '|', 1)),
                    "SourcePublishedAtUtc");
                """);

            migrationBuilder.Sql("DROP FUNCTION pg_temp.cafri_parse_publication_date(text);");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "PublicationContentItems");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationContentItems_PublishedAtUtc",
                table: "PublicationContentItems",
                column: "PublishedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PublicationContentItems_PublishedAtUtc",
                table: "PublicationContentItems");

            migrationBuilder.AddColumn<string>(
                name: "PublishedDate",
                table: "PublicationContentItems",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "PublicationContentItems"
                SET "PublishedDate" = coalesce(to_char("PublishedAtUtc" AT TIME ZONE 'UTC', 'FMMonth YYYY'), '');
                """);

            migrationBuilder.DropColumn(
                name: "PublishedAtUtc",
                table: "PublicationContentItems");
        }
    }
}
