using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteCascadeIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_AspNetUsers_OwnerId",
                table: "Attachment");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Mail_MailId",
                table: "Attachment");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_AspNetUsers_OwnerId",
                table: "Attachment",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Mail_MailId",
                table: "Attachment",
                column: "MailId",
                principalTable: "Mail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_AspNetUsers_OwnerId",
                table: "Attachment");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Mail_MailId",
                table: "Attachment");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_AspNetUsers_OwnerId",
                table: "Attachment",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Mail_MailId",
                table: "Attachment",
                column: "MailId",
                principalTable: "Mail",
                principalColumn: "Id");
        }
    }
}
