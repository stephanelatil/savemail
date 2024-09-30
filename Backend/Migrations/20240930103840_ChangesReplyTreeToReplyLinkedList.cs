using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangesReplyTreeToReplyLinkedList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mail_Mail_RepliedFromId",
                table: "Mail");

            migrationBuilder.DropIndex(
                name: "IX_Mail_RepliedFromId",
                table: "Mail");

            migrationBuilder.AddColumn<bool>(
                name: "HasReply",
                table: "Mail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "ReplyId",
                table: "Mail",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mail_ReplyId",
                table: "Mail",
                column: "ReplyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_Mail_ReplyId",
                table: "Mail",
                column: "ReplyId",
                principalTable: "Mail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mail_Mail_ReplyId",
                table: "Mail");

            migrationBuilder.DropIndex(
                name: "IX_Mail_ReplyId",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "HasReply",
                table: "Mail");

            migrationBuilder.DropColumn(
                name: "ReplyId",
                table: "Mail");

            migrationBuilder.CreateIndex(
                name: "IX_Mail_RepliedFromId",
                table: "Mail",
                column: "RepliedFromId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mail_Mail_RepliedFromId",
                table: "Mail",
                column: "RepliedFromId",
                principalTable: "Mail",
                principalColumn: "Id");
        }
    }
}
