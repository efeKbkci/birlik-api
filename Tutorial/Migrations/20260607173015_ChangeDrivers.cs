using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDrivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_active",
                table: "drivers");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "drivers",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "drivers");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "drivers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
