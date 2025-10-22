using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMCSApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClaimStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoordinatorStatus",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ManagerStatus",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoordinatorStatus",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ManagerStatus",
                table: "Claims");
        }
    }
}
