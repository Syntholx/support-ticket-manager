using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportTicketManager.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceDescriptionLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Tickets_DescriptionLength",
                table: "Tickets",
                sql: "DATALENGTH([Description]) <= 10000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Tickets_DescriptionLength",
                table: "Tickets");
        }
    }
}
