using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class _005 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComment_AspNetUsers_UserId1",
                table: "TicketComment");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "TicketComment",
                newName: "ModeratorId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketComment_UserId1",
                table: "TicketComment",
                newName: "IX_TicketComment_ModeratorId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "TicketComment",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "Moderated",
                table: "TicketComment",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModeratedBody",
                table: "TicketComment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModeratedReason",
                table: "TicketComment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModerationType",
                table: "TicketComment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "TicketComment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "TicketComment",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_TicketComment_UserId",
                table: "TicketComment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComment_AspNetUsers_ModeratorId",
                table: "TicketComment",
                column: "ModeratorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComment_AspNetUsers_UserId",
                table: "TicketComment",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComment_AspNetUsers_ModeratorId",
                table: "TicketComment");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketComment_AspNetUsers_UserId",
                table: "TicketComment");

            migrationBuilder.DropIndex(
                name: "IX_TicketComment_UserId",
                table: "TicketComment");

            migrationBuilder.DropColumn(
                name: "Moderated",
                table: "TicketComment");

            migrationBuilder.DropColumn(
                name: "ModeratedBody",
                table: "TicketComment");

            migrationBuilder.DropColumn(
                name: "ModeratedReason",
                table: "TicketComment");

            migrationBuilder.DropColumn(
                name: "ModerationType",
                table: "TicketComment");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "TicketComment");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "TicketComment");

            migrationBuilder.RenameColumn(
                name: "ModeratorId",
                table: "TicketComment",
                newName: "UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_TicketComment_ModeratorId",
                table: "TicketComment",
                newName: "IX_TicketComment_UserId1");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "TicketComment",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComment_AspNetUsers_UserId1",
                table: "TicketComment",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
