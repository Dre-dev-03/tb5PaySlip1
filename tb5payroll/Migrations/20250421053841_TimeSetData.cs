using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tb5payroll.Migrations
{
    /// <inheritdoc />
    public partial class TimeSetData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "underTimeDeductionEmployeeData",
                table: "EmployeeData",
                type: "decimal(7,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "taxEmployeeData",
                table: "EmployeeData",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "lateDeductionEmployeeData",
                table: "EmployeeData",
                type: "decimal(7,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "calculatedPayEmployeeData",
                table: "EmployeeData",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeArchive",
                columns: table => new
                {
                    idEmployeeData = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    employeeArchiveData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    birthdayEmployeeData = table.Column<int>(type: "int", nullable: false),
                    nameEmployeeData = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    basePayEmployeeData = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    hoursWorkedEmployeeData = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    overtimeHoursEmployeeData = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    holidayHoursEmployeeData = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    trainingEmployeeData = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    cashAdvEmployeeData = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    loanEmployeeData = table.Column<decimal>(type: "decimal(7,2)", nullable: true),
                    sssEmployeeData = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    pagIbigEmployeeData = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    philHealthEmployeeData = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    taxEmployeeData = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    calculatedPayEmployeeData = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeArchive", x => x.idEmployeeData);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TimeSetData",
                columns: table => new
                {
                    timeSetData = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    idEmployeeData = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    undertimeThreshold = table.Column<int>(type: "int", nullable: true),
                    overtimeThreshold = table.Column<int>(type: "int", nullable: true),
                    earlyoutThreshold = table.Column<int>(type: "int", nullable: true),
                    overtimeMultiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    halfdayMultiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSetData", x => x.timeSetData);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeArchive");

            migrationBuilder.DropTable(
                name: "TimeSetData");

            migrationBuilder.AlterColumn<decimal>(
                name: "underTimeDeductionEmployeeData",
                table: "EmployeeData",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "taxEmployeeData",
                table: "EmployeeData",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "lateDeductionEmployeeData",
                table: "EmployeeData",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "calculatedPayEmployeeData",
                table: "EmployeeData",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);
        }
    }
}
