using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class _006 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
