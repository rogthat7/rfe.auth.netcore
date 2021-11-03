using Microsoft.EntityFrameworkCore.Migrations;

namespace RFE.Auth.API.Migrations
{
    public partial class AddedForeingnKeystoUserAppPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserAppPermission_AppId",
                schema: "AUTH",
                table: "UserAppPermission",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAppPermission_PermissionId",
                schema: "AUTH",
                table: "UserAppPermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAppPermission_UserId",
                schema: "AUTH",
                table: "UserAppPermission",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAppPermission_Application_AppId",
                schema: "AUTH",
                table: "UserAppPermission",
                column: "AppId",
                principalSchema: "AUTH",
                principalTable: "Application",
                principalColumn: "AppId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAppPermission_AppPermission_PermissionId",
                schema: "AUTH",
                table: "UserAppPermission",
                column: "PermissionId",
                principalSchema: "AUTH",
                principalTable: "AppPermission",
                principalColumn: "PermissionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAppPermission_AuthUser_UserId",
                schema: "AUTH",
                table: "UserAppPermission",
                column: "UserId",
                principalSchema: "AUTH",
                principalTable: "AuthUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAppPermission_Application_AppId",
                schema: "AUTH",
                table: "UserAppPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAppPermission_AppPermission_PermissionId",
                schema: "AUTH",
                table: "UserAppPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAppPermission_AuthUser_UserId",
                schema: "AUTH",
                table: "UserAppPermission");

            migrationBuilder.DropIndex(
                name: "IX_UserAppPermission_AppId",
                schema: "AUTH",
                table: "UserAppPermission");

            migrationBuilder.DropIndex(
                name: "IX_UserAppPermission_PermissionId",
                schema: "AUTH",
                table: "UserAppPermission");

            migrationBuilder.DropIndex(
                name: "IX_UserAppPermission_UserId",
                schema: "AUTH",
                table: "UserAppPermission");
        }
    }
}
