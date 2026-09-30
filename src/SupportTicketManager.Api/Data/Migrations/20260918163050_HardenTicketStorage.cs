using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportTicketManager.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class HardenTicketStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Tickets] WHERE DATALENGTH([Title]) > 400 OR DATALENGTH([Description]) > 10000)
                    THROW 51000, 'Existing tickets exceed the new limits. Review the data before migration; no truncation is performed.', 1;
                """);
            migrationBuilder.DropIndex(
                name: "IX_Tickets_OwnerId",
                table: "Tickets");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tickets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tickets",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OwnerId_Status_Priority_Id",
                table: "Tickets",
                columns: new[] { "OwnerId", "Status", "Priority", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_OwnerId_Status_Priority_Id",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Tickets");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_OwnerId",
                table: "Tickets",
                column: "OwnerId");
        }
    }
}
