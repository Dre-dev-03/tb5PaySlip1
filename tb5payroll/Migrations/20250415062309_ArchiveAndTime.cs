using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tb5payroll.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveAndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "calculatedPayEmployeeData",
                table: "EmployeeData",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "lateDeductionEmployeeData",
                table: "EmployeeData",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "taxEmployeeData",
                table: "EmployeeData",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "underTimeDeductionEmployeeData",
                table: "EmployeeData",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "calculatedPayEmployeeData",
                table: "EmployeeData");

            migrationBuilder.DropColumn(
                name: "lateDeductionEmployeeData",
                table: "EmployeeData");

            migrationBuilder.DropColumn(
                name: "taxEmployeeData",
                table: "EmployeeData");

            migrationBuilder.DropColumn(
                name: "underTimeDeductionEmployeeData",
                table: "EmployeeData");
        }
    }
}
