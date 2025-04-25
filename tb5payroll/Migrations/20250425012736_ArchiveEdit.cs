using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tb5payroll.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeArchive",
                table: "EmployeeArchive");

            migrationBuilder.DropColumn(
                name: "employeeArchiveData",
                table: "EmployeeArchive");

            migrationBuilder.AlterColumn<string>(
                name: "nameEmployeeData",
                table: "EmployeeArchive",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "birthdayEmployeeData",
                table: "EmployeeArchive",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "basePayEmployeeData",
                table: "EmployeeArchive",
                type: "decimal(65,30)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<string>(
                name: "idEmployeeData",
                table: "EmployeeArchive",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "archiveId",
                table: "EmployeeArchive",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "employeeArchiveDate",
                table: "EmployeeArchive",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeArchive",
                table: "EmployeeArchive",
                column: "archiveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeArchive",
                table: "EmployeeArchive");

            migrationBuilder.DropColumn(
                name: "archiveId",
                table: "EmployeeArchive");

            migrationBuilder.DropColumn(
                name: "employeeArchiveDate",
                table: "EmployeeArchive");

            migrationBuilder.UpdateData(
                table: "EmployeeArchive",
                keyColumn: "nameEmployeeData",
                keyValue: null,
                column: "nameEmployeeData",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "nameEmployeeData",
                table: "EmployeeArchive",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "EmployeeArchive",
                keyColumn: "idEmployeeData",
                keyValue: null,
                column: "idEmployeeData",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "idEmployeeData",
                table: "EmployeeArchive",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "birthdayEmployeeData",
                table: "EmployeeArchive",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "basePayEmployeeData",
                table: "EmployeeArchive",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "employeeArchiveData",
                table: "EmployeeArchive",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeArchive",
                table: "EmployeeArchive",
                column: "idEmployeeData");
        }
    }
}
