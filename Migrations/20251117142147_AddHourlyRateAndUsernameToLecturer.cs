using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMCSApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddHourlyRateAndUsernameToLecturer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Lecturers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Lecturers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Lecturers");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Claims");
        }
    }
}
