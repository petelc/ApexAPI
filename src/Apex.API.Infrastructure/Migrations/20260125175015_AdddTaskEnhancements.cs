using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apex.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdddTaskEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompletedByUserId",
                schema: "shared",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImplementationNotes",
                schema: "shared",
                table: "Tasks",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolutionNotes",
                schema: "shared",
                table: "Tasks",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StartedByUserId",
                schema: "shared",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TaskActivityLogs",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskActivityLogs_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "shared",
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskChecklistItems",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskChecklistItems_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "shared",
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CompletedByUserId",
                schema: "shared",
                table: "Tasks",
                column: "CompletedByUserId",
                filter: "[CompletedByUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_StartedByUserId",
                schema: "shared",
                table: "Tasks",
                column: "StartedByUserId",
                filter: "[StartedByUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActivityLogs_ActivityType",
                schema: "shared",
                table: "TaskActivityLogs",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActivityLogs_TaskId",
                schema: "shared",
                table: "TaskActivityLogs",
                column: "TaskId");

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

            migrationBuilder.CreateIndex(
                name: "IX_TaskChecklistItems_TaskId",
                schema: "shared",
                table: "TaskChecklistItems",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChecklistItems_TaskId_Order",
                schema: "shared",
                table: "TaskChecklistItems",
                columns: new[] { "TaskId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskActivityLogs",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "TaskChecklistItems",
                schema: "shared");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CompletedByUserId",
                schema: "shared",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_StartedByUserId",
                schema: "shared",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                schema: "shared",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ImplementationNotes",
                schema: "shared",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ResolutionNotes",
                schema: "shared",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "StartedByUserId",
                schema: "shared",
                table: "Tasks");
        }
    }
}
