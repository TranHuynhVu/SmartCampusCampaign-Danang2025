using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCD_2025_BE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentinfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "StudentInfors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpenToWork",
                table: "StudentInfors",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "StudentInfors");

            migrationBuilder.DropColumn(
                name: "OpenToWork",
                table: "StudentInfors");
        }
    }
}
