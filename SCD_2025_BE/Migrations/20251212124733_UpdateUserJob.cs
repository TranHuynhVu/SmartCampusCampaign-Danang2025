using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCD_2025_BE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "UserJobs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserJobs");
        }
    }
}
