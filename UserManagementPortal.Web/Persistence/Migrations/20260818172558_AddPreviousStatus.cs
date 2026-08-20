using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementPortal.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviousStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Users",
                newName: "PreviousStatus");

            migrationBuilder.AddColumn<int>(
                name: "CurrentStatus",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PreviousStatus",
                table: "Users",
                newName: "Status");
        }
    }
}
