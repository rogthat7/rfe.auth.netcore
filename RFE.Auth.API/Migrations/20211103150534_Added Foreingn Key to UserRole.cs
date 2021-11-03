using Microsoft.EntityFrameworkCore.Migrations;

namespace RFE.Auth.API.Migrations
{
    public partial class AddedForeingnKeytoUserRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                schema: "AUTH",
                table: "UserRole",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_AuthUser_UserId",
                schema: "AUTH",
                table: "UserRole",
                column: "UserId",
                principalSchema: "AUTH",
                principalTable: "AuthUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_AuthUser_UserId",
                schema: "AUTH",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_UserRole_UserId",
                schema: "AUTH",
                table: "UserRole");
        }
    }
}
