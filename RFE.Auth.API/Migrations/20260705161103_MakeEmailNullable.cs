using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RFE.Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmailNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "AUTH",
                table: "AuthUser",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_AuthUser_Phone",
                schema: "AUTH",
                table: "AuthUser",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuthUser_Phone",
                schema: "AUTH",
                table: "AuthUser");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "AUTH",
                table: "AuthUser",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
