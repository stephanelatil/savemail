using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class SameMailsCanBeInMultipleFoldersToAvoidDuplicates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mail_Folder_FolderId",
                table: "Mail");

            migrationBuilder.DropIndex(
                name: "IX_Mail_FolderId",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "Mail");

            migrationBuilder.AddColumn<decimal>(
                name: "UniqueHash2",
                table: "Mail",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FolderMail",
                columns: table => new
                {
                    FolderId = table.Column<int>(type: "integer", nullable: false),
                    MailsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderMail", x => new { x.FolderId, x.MailsId });
                    table.ForeignKey(
                        name: "FK_FolderMail_Folder_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FolderMail_Mail_MailsId",
                        column: x => x.MailsId,
                        principalTable: "Mail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FolderMail_MailsId",
                table: "FolderMail",
                column: "MailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FolderMail");

            migrationBuilder.DropColumn(
                name: "UniqueHash2",
                table: "Mail");

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "Mail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Mail_FolderId",
                table: "Mail",
                column: "FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_Folder_FolderId",
                table: "Mail",
                column: "FolderId",
                principalTable: "Folder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
