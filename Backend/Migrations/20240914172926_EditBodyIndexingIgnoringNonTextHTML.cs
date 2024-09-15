using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class EditBodyIndexingIgnoringNonTextHTML : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyText",
                table: "Mail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Mail",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(\"Subject\", '') || ' ' || coalesce(\"BodyText\", ''))",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldComputedColumnSql: "to_tsvector('english', coalesce(\"Subject\", '') || ' ' || coalesce(\"Body\", ''))",
                oldStored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyText",
                table: "Mail");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Mail",
                type: "tsvector",
                nullable: false,
                computedColumnSql: "to_tsvector('english', coalesce(\"Subject\", '') || ' ' || coalesce(\"Body\", ''))",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true,
                oldComputedColumnSql: "to_tsvector('english', coalesce(\"Subject\", '') || ' ' || coalesce(\"BodyText\", ''))",
                oldStored: true);
        }
    }
}
