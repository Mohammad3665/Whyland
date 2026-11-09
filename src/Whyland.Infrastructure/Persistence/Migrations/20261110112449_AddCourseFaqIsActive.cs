using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Whyland.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseFaqIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "catalog",
                table: "CourseFaqs",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "catalog",
                table: "CourseFaqs");
        }
    }
}
