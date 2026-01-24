using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Apex.API.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPRDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedBudget",
                schema: "shared",
                table: "ProjectRequests",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProposedEndDate",
                schema: "shared",
                table: "ProjectRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProposedStartDate",
                schema: "shared",
                table: "ProjectRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedBudget",
                schema: "shared",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "ProposedEndDate",
                schema: "shared",
                table: "ProjectRequests");

            migrationBuilder.DropColumn(
                name: "ProposedStartDate",
                schema: "shared",
                table: "ProjectRequests");
        }
    }
}
