using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apex.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "shared",
                table: "Projects",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                schema: "shared",
                table: "Projects",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "shared",
                table: "ProjectRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                schema: "shared",
                table: "ProjectRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedToDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    ActualHours = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BlockedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToDepartmentId",
                schema: "shared",
                table: "Tasks",
                column: "AssignedToDepartmentId",
                filter: "[AssignedToDepartmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToDepartmentId_Status",
                schema: "shared",
                table: "Tasks",
                columns: new[] { "AssignedToDepartmentId", "Status" },
                filter: "[AssignedToDepartmentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToUserId",
                schema: "shared",
                table: "Tasks",
                column: "AssignedToUserId",
                filter: "[AssignedToUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                schema: "shared",
                table: "Tasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId_Status",
                schema: "shared",
                table: "Tasks",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TenantId",
                schema: "shared",
                table: "Tasks",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "shared");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "shared",
                table: "Projects",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                schema: "shared",
                table: "Projects",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "shared",
                table: "ProjectRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                schema: "shared",
                table: "ProjectRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
