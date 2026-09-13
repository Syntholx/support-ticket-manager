using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportTicketManager.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketPriorityConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Tickets_Priority",
                table: "Tickets",
                sql: "[Priority] >=1 AND [Priority] <= 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Tickets_Priority",
                table: "Tickets");
        }
    }
}
