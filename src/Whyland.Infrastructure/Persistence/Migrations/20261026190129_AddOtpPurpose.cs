using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Whyland.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpPurpose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                schema: "identity",
                table: "OtpRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OtpRequests_PhoneNumber_Purpose_CreatedAt",
                schema: "identity",
                table: "OtpRequests",
                columns: new[] { "PhoneNumber", "Purpose", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OtpRequests_PhoneNumber_Purpose_CreatedAt",
                schema: "identity",
                table: "OtpRequests");

            migrationBuilder.DropColumn(
                name: "Purpose",
                schema: "identity",
                table: "OtpRequests");
        }
    }
}
