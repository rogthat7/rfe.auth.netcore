using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RFE.Auth.API.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationDetailsAndAppRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "AUTH",
                table: "Application",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "AUTH",
                table: "Application",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "AUTH",
                table: "Application",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WebhookUrl",
                schema: "AUTH",
                table: "Application",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppRole",
                schema: "AUTH",
                columns: table => new
                {
                    AppRoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRole", x => x.AppRoleId);
                    table.ForeignKey(
                        name: "FK_AppRole_Application_AppId",
                        column: x => x.AppId,
                        principalSchema: "AUTH",
                        principalTable: "Application",
                        principalColumn: "AppId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppRole_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AUTH",
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRole_AppId",
                schema: "AUTH",
                table: "AppRole",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRole_RoleId",
                schema: "AUTH",
                table: "AppRole",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRole",
                schema: "AUTH");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "AUTH",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "AUTH",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "AUTH",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "WebhookUrl",
                schema: "AUTH",
                table: "Application");
        }
    }
}
