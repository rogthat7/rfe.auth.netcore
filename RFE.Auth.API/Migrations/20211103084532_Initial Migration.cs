using Microsoft.EntityFrameworkCore.Migrations;

namespace RFE.Auth.API.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AUTH");

            migrationBuilder.CreateTable(
                name: "Application",
                schema: "AUTH",
                columns: table => new
                {
                    AppId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppName = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Application", x => x.AppId);
                });

            migrationBuilder.CreateTable(
                name: "AppPermission",
                schema: "AUTH",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PermissionType = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPermission", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "AuthUser",
                schema: "AUTH",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Phone = table.Column<long>(type: "bigint", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthUser", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "AUTH",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "UserAppPermission",
                schema: "AUTH",
                columns: table => new
                {
                    UAPId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AppId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAppPermission", x => x.UAPId);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                schema: "AUTH",
                columns: table => new
                {
                    UserRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    AppId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.UserRoleId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Application_AppName",
                schema: "AUTH",
                table: "Application",
                column: "AppName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPermission_PermissionName",
                schema: "AUTH",
                table: "AppPermission",
                column: "PermissionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPermission_PermissionType",
                schema: "AUTH",
                table: "AppPermission",
                column: "PermissionType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthUser_Email",
                schema: "AUTH",
                table: "AuthUser",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthUser_Username",
                schema: "AUTH",
                table: "AuthUser",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Application",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "AppPermission",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "AuthUser",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "UserAppPermission",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "UserRole",
                schema: "AUTH");
        }
    }
}
