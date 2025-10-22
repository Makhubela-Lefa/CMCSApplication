using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMCSApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModuleAssignments_Lecturer_LecturerId",
                table: "ModuleAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lecturer",
                table: "Lecturer");

            migrationBuilder.RenameTable(
                name: "Lecturer",
                newName: "Lecturers");

            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "Claims",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lecturers",
                table: "Lecturers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ModuleId",
                table: "Claims",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Modules_ModuleId",
                table: "Claims",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleAssignments_Lecturers_LecturerId",
                table: "ModuleAssignments",
                column: "LecturerId",
                principalTable: "Lecturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Modules_ModuleId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_ModuleAssignments_Lecturers_LecturerId",
                table: "ModuleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Claims_ModuleId",
                table: "Claims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lecturers",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Claims");

            migrationBuilder.RenameTable(
                name: "Lecturers",
                newName: "Lecturer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lecturer",
                table: "Lecturer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModuleAssignments_Lecturer_LecturerId",
                table: "ModuleAssignments",
                column: "LecturerId",
                principalTable: "Lecturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
