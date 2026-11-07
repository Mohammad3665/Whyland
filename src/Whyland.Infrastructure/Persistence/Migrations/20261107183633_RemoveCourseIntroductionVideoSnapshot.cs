using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Whyland.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCourseIntroductionVideoSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The column was already dropped in an earlier, now-missing migration that the database
            // still records in its history. IF EXISTS lets this migration repair the snapshot on an
            // existing database and still drop the column on a fresh one.
            migrationBuilder.Sql("ALTER TABLE catalog.\"Courses\" DROP COLUMN IF EXISTS \"IntroductionVideo\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntroductionVideo",
                schema: "catalog",
                table: "Courses",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }
    }
}
