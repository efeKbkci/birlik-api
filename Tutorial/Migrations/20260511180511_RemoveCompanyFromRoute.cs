using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanyFromRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_routes_companies_company_id",
                table: "routes");

            migrationBuilder.DropIndex(
                name: "ix_routes_company_id",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "base_price",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "routes");

            migrationBuilder.AddColumn<int>(
                name: "company_id",
                table: "stops",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_stops_company_id",
                table: "stops",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_stops_companies_company_id",
                table: "stops",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_stops_companies_company_id",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "ix_stops_company_id",
                table: "stops");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "stops");

            migrationBuilder.AddColumn<int>(
                name: "base_price",
                table: "routes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "company_id",
                table: "routes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_routes_company_id",
                table: "routes",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "fk_routes_companies_company_id",
                table: "routes",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
