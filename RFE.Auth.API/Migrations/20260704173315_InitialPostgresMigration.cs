using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RFE.Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AUTH");

            migrationBuilder.CreateTable(
                name: "Application",
                schema: "AUTH",
                columns: table => new
                {
                    AppId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppName = table.Column<string>(type: "text", nullable: false)
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
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PermissionName = table.Column<string>(type: "text", nullable: false),
                    PermissionType = table.Column<string>(type: "text", nullable: false)
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
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<long>(type: "bigint", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: false)
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
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "text", nullable: true)
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
                    UAPId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AppId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAppPermission", x => x.UAPId);
                    table.ForeignKey(
                        name: "FK_UserAppPermission_AppPermission_PermissionId",
                        column: x => x.PermissionId,
                        principalSchema: "AUTH",
                        principalTable: "AppPermission",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAppPermission_Application_AppId",
                        column: x => x.AppId,
                        principalSchema: "AUTH",
                        principalTable: "Application",
                        principalColumn: "AppId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAppPermission_AuthUser_UserId",
                        column: x => x.UserId,
                        principalSchema: "AUTH",
                        principalTable: "AuthUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                schema: "AUTH",
                columns: table => new
                {
                    UserRoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    AppId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_UserRole_AuthUser_UserId",
                        column: x => x.UserId,
                        principalSchema: "AUTH",
                        principalTable: "AuthUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                schema: "AUTH",
                table: "UserRole",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Roles",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "UserAppPermission",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "UserRole",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "AppPermission",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "Application",
                schema: "AUTH");

            migrationBuilder.DropTable(
                name: "AuthUser",
                schema: "AUTH");
        }
    }
}
