using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCD_2025_BE.Migrations
{
    /// <inheritdoc />
    public partial class addStudentInforEducations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Educations",
                table: "StudentInfors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Educations",
                table: "StudentInfors");
        }
    }
}
