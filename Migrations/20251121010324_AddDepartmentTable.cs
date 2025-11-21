using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMCSApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Lecturers");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Lecturers",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lecturers_DepartmentId",
                table: "Lecturers",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lecturers_Departments_DepartmentId",
                table: "Lecturers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lecturers_Departments_DepartmentId",
                table: "Lecturers");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Lecturers_DepartmentId",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Lecturers");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Lecturers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
