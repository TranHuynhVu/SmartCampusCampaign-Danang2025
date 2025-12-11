using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCD_2025_BE.Migrations
{
    /// <inheritdoc />
    public partial class removeDescriptionCompanyInfor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CompanyInfors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CompanyInfors",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
