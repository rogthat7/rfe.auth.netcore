using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RFE.Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVerifiedToAuthUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                schema: "AUTH",
                table: "AuthUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                schema: "AUTH",
                table: "AuthUser");
        }
    }
}
