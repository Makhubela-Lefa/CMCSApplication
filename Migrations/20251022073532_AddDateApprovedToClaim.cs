using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMCSApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddDateApprovedToClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateApproved",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateVerified",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateApproved",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DateVerified",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Claims");
        }
    }
}
