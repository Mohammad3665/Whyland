#nullable disable

namespace Whyland.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EditedUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "identity",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "identity",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "NationalCode",
                schema: "identity",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "identity",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Province",
                schema: "identity",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "identity",
                table: "InstructorProfiles");

            migrationBuilder.DropColumn(
                name: "NationalCode",
                schema: "identity",
                table: "InstructorProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "identity",
                table: "UserProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "identity",
                table: "UserProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalCode",
                schema: "identity",
                table: "UserProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "identity",
                table: "UserProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                schema: "identity",
                table: "UserProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "identity",
                table: "InstructorProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalCode",
                schema: "identity",
                table: "InstructorProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
