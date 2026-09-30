using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportTicketManager.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AccountEmailCooldown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastConfirmationEmailAt",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastPasswordResetEmailAt",
                table: "AspNetUsers",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastConfirmationEmailAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastPasswordResetEmailAt",
                table: "AspNetUsers");
        }
    }
}
