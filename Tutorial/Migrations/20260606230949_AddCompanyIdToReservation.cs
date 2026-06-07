using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "company_id",
                table: "reservations",
                type: "integer",
                nullable: false,
                defaultValue: 12);

            migrationBuilder.CreateIndex(
                name: "ix_reservations_company_id",
                table: "reservations",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_reservations_companies_company_id",
                table: "reservations",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_reservations_companies_company_id",
                table: "reservations");

            migrationBuilder.DropIndex(
                name: "ix_reservations_company_id",
                table: "reservations");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "reservations");
        }
    }
}
