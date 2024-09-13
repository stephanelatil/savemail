using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangesDateTimeToLongForCompatibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use two step migration

            // Add new columns with the type `bigint` to store converted DateTime values
            migrationBuilder.AddColumn<long>(
                name: "AccessTokenValidityLong",
                table: "OAuthCredentials",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DateSentLong",
                table: "Mail",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LastPulledInternalDateLong",
                table: "Folder",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Backfill data into the new columns using Unix timestamp conversion
            migrationBuilder.Sql(@"
                UPDATE ""OAuthCredentials""
                SET ""AccessTokenValidityLong"" = EXTRACT(EPOCH FROM ""AccessTokenValidity"")::bigint;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Mail""
                SET ""DateSentLong"" = EXTRACT(EPOCH FROM ""DateSent"")::bigint;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Folder""
                SET ""LastPulledInternalDateLong"" = EXTRACT(EPOCH FROM ""LastPulledInternalDate"")::bigint;
            ");

            // Drop the old DateTime columns
            migrationBuilder.DropColumn(
                name: "AccessTokenValidity",
                table: "OAuthCredentials");

            migrationBuilder.DropColumn(
                name: "DateSent",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "LastPulledInternalDate",
                table: "Folder");

            // Rename the new bigint columns to replace the old DateTime columns
            migrationBuilder.RenameColumn(
                name: "AccessTokenValidityLong",
                table: "OAuthCredentials",
                newName: "AccessTokenValidity");

            migrationBuilder.RenameColumn(
                name: "DateSentLong",
                table: "Mail",
                newName: "DateSent");

            migrationBuilder.RenameColumn(
                name: "LastPulledInternalDateLong",
                table: "Folder",
                newName: "LastPulledInternalDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add back the original DateTime columns
            migrationBuilder.AddColumn<DateTime>(
                name: "AccessTokenValidity",
                table: "OAuthCredentials",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSent",
                table: "Mail",
                type: "timestamp(6) without time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPulledInternalDate",
                table: "Folder",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            // Backfill the DateTime columns with the converted values from the bigint columns
            migrationBuilder.Sql(@"
                UPDATE ""OAuthCredentials""
                SET ""AccessTokenValidity"" = to_timestamp(""AccessTokenValidity"");
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Mail""
                SET ""DateSent"" = to_timestamp(""DateSent"");
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Folder""
                SET ""LastPulledInternalDate"" = to_timestamp(""LastPulledInternalDate"");
            ");

            // Drop the bigint columns
            migrationBuilder.DropColumn(
                name: "AccessTokenValidity",
                table: "OAuthCredentials");

            migrationBuilder.DropColumn(
                name: "DateSent",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "LastPulledInternalDate",
                table: "Folder");

            // Rename the original DateTime columns back to their original names
            migrationBuilder.RenameColumn(
                name: "AccessTokenValidity",
                table: "OAuthCredentials",
                newName: "AccessTokenValidityLong");

            migrationBuilder.RenameColumn(
                name: "DateSent",
                table: "Mail",
                newName: "DateSentLong");

            migrationBuilder.RenameColumn(
                name: "LastPulledInternalDate",
                table: "Folder",
                newName: "LastPulledInternalDateLong");
        }
    }
}
