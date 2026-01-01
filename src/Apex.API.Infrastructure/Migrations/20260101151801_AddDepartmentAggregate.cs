using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apex.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                schema: "shared",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Email",
                schema: "shared",
                table: "Users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "shared",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                schema: "shared",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Departments",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DepartmentManagerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                schema: "shared",
                table: "Users",
                column: "DepartmentId",
                filter: "[DepartmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                schema: "shared",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_DepartmentId",
                schema: "shared",
                table: "Users",
                columns: new[] { "TenantId", "DepartmentId" },
                filter: "[DepartmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DepartmentManagerUserId",
                schema: "shared",
                table: "Departments",
                column: "DepartmentManagerUserId",
                filter: "[DepartmentManagerUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_TenantId",
                schema: "shared",
                table: "Departments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_TenantId_IsActive",
                schema: "shared",
                table: "Departments",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_TenantId_Name",
                schema: "shared",
                table: "Departments",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Departments",
                schema: "shared");

            migrationBuilder.DropIndex(
                name: "IX_Users_DepartmentId",
                schema: "shared",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                schema: "shared",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_DepartmentId",
                schema: "shared",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "shared",
                table: "Users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "shared",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "shared",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                schema: "shared",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[Email] IS NOT NULL");
        }
    }
}
