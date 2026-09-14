using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportTicketManager.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OwnerId",
                table: "Tickets",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_OwnerId",
                table: "Tickets",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_OwnerId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_OwnerId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Tickets");
        }
    }
}
