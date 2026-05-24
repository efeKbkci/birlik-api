using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTripTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "available_capacity",
                table: "trips",
                newName: "passenger_numbers");

            migrationBuilder.AddColumn<int>(
                name: "capacity",
                table: "trips",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "capacity",
                table: "trips");

            migrationBuilder.RenameColumn(
                name: "passenger_numbers",
                table: "trips",
                newName: "available_capacity");
        }
    }
}
