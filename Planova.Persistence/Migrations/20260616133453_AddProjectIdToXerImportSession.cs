using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectIdToXerImportSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "XerImportSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddDate",
                table: "PrimaveraProjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRecalcDate",
                table: "PrimaveraProjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastScheduleDate",
                table: "PrimaveraProjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTasksumDate",
                table: "PrimaveraProjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanEndDate",
                table: "PrimaveraProjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanStartDate",
                table: "PrimaveraProjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SchedEndDate",
                table: "PrimaveraProjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivityCode",
                table: "PrimaveraActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndDate",
                table: "PrimaveraActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartDate",
                table: "PrimaveraActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EarlyEndDate",
                table: "PrimaveraActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EarlyStartDate",
                table: "PrimaveraActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FreeFloat",
                table: "PrimaveraActivities",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LateEndDate",
                table: "PrimaveraActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LateStartDate",
                table: "PrimaveraActivities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OriginalDuration",
                table: "PrimaveraActivities",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalFloat",
                table: "PrimaveraActivities",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_XerImportSessions_ProjectId",
                table: "XerImportSessions",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XerImportSessions_ProjectId",
                table: "XerImportSessions");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "XerImportSessions");

            migrationBuilder.DropColumn(
                name: "AddDate",
                table: "PrimaveraProjects");

            migrationBuilder.DropColumn(
                name: "LastRecalcDate",
                table: "PrimaveraProjects");

            migrationBuilder.DropColumn(
                name: "LastScheduleDate",
                table: "PrimaveraProjects");

            migrationBuilder.DropColumn(
                name: "LastTasksumDate",
                table: "PrimaveraProjects");

            migrationBuilder.DropColumn(
                name: "PlanEndDate",
                table: "PrimaveraProjects");

            migrationBuilder.DropColumn(
                name: "PlanStartDate",
                table: "PrimaveraProjects");

            migrationBuilder.DropColumn(
                name: "SchedEndDate",
                table: "PrimaveraProjects");

            migrationBuilder.DropColumn(
                name: "ActivityCode",
                table: "PrimaveraActivities");

            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "PrimaveraActivities");

            migrationBuilder.DropColumn(
                name: "ActualStartDate",
                table: "PrimaveraActivities");

            migrationBuilder.DropColumn(
                name: "EarlyEndDate",
                table: "PrimaveraActivities");

            migrationBuilder.DropColumn(
                name: "EarlyStartDate",
                table: "PrimaveraActivities");

            migrationBuilder.DropColumn(
                name: "FreeFloat",
                table: "PrimaveraActivities");

            migrationBuilder.DropColumn(
                name: "LateEndDate",
                table: "PrimaveraActivities");

            migrationBuilder.DropColumn(
                name: "LateStartDate",
                table: "PrimaveraActivities");

            migrationBuilder.DropColumn(
                name: "OriginalDuration",
                table: "PrimaveraActivities");

            migrationBuilder.DropColumn(
                name: "TotalFloat",
                table: "PrimaveraActivities");
        }
    }
}
