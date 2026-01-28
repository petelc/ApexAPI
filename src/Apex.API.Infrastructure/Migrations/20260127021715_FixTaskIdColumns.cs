using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apex.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTaskIdColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskChecklistItems_TaskId_Order",
                schema: "shared",
                table: "TaskChecklistItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskActivityLogs_ActivityType",
                schema: "shared",
                table: "TaskActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskActivityLogs_TaskId_ActivityType_Timestamp",
                schema: "shared",
                table: "TaskActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskActivityLogs_TaskId_Timestamp",
                schema: "shared",
                table: "TaskActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskActivityLogs_UserId",
                schema: "shared",
                table: "TaskActivityLogs");

            migrationBuilder.RenameTable(
                name: "TaskChecklistItems",
                schema: "shared",
                newName: "TaskChecklistItems");

            migrationBuilder.RenameTable(
                name: "TaskActivityLogs",
                schema: "shared",
                newName: "TaskActivityLogs");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "TaskChecklistItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "TaskChecklistItems",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TaskId2",
                table: "TaskChecklistItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "TaskActivityLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldMaxLength: 5000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TaskActivityLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<Guid>(
                name: "TaskId2",
                table: "TaskActivityLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskChecklistItems_TaskId2",
                table: "TaskChecklistItems",
                column: "TaskId2");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActivityLogs_TaskId2",
                table: "TaskActivityLogs",
                column: "TaskId2");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActivityLogs_Timestamp",
                table: "TaskActivityLogs",
                column: "Timestamp");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskActivityLogs_Tasks_TaskId2",
                table: "TaskActivityLogs",
                column: "TaskId2",
                principalSchema: "shared",
                principalTable: "Tasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskChecklistItems_Tasks_TaskId2",
                table: "TaskChecklistItems",
                column: "TaskId2",
                principalSchema: "shared",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskActivityLogs_Tasks_TaskId2",
                table: "TaskActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskChecklistItems_Tasks_TaskId2",
                table: "TaskChecklistItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskChecklistItems_TaskId2",
                table: "TaskChecklistItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskActivityLogs_TaskId2",
                table: "TaskActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskActivityLogs_Timestamp",
                table: "TaskActivityLogs");

            migrationBuilder.DropColumn(
                name: "TaskId2",
                table: "TaskChecklistItems");

            migrationBuilder.DropColumn(
                name: "TaskId2",
                table: "TaskActivityLogs");

            migrationBuilder.RenameTable(
                name: "TaskChecklistItems",
                newName: "TaskChecklistItems",
                newSchema: "shared");

            migrationBuilder.RenameTable(
                name: "TaskActivityLogs",
                newName: "TaskActivityLogs",
                newSchema: "shared");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                schema: "shared",
                table: "TaskChecklistItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                schema: "shared",
                table: "TaskChecklistItems",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                schema: "shared",
                table: "TaskActivityLogs",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "shared",
                table: "TaskActivityLogs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChecklistItems_TaskId_Order",
                schema: "shared",
                table: "TaskChecklistItems",
                columns: new[] { "TaskId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskActivityLogs_ActivityType",
                schema: "shared",
                table: "TaskActivityLogs",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActivityLogs_TaskId_ActivityType_Timestamp",
                schema: "shared",
                table: "TaskActivityLogs",
                columns: new[] { "TaskId", "ActivityType", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskActivityLogs_TaskId_Timestamp",
                schema: "shared",
                table: "TaskActivityLogs",
                columns: new[] { "TaskId", "Timestamp" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TaskActivityLogs_UserId",
                schema: "shared",
                table: "TaskActivityLogs",
                column: "UserId");
        }
    }
}
