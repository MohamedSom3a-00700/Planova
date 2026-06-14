using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimaveraEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrimaveraActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    WbsId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Duration = table.Column<double>(type: "REAL", nullable: false),
                    RemainingDuration = table.Column<double>(type: "REAL", nullable: false),
                    PercentComplete = table.Column<double>(type: "REAL", nullable: false),
                    CalendarId = table.Column<string>(type: "TEXT", nullable: true),
                    BaselineId = table.Column<Guid>(type: "TEXT", nullable: true),
                    BaselineVersion = table.Column<int>(type: "INTEGER", nullable: true),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UdfValues = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraBaselines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    BaselineId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraBaselines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraCalendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    CalendarId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsBaseCalendar = table.Column<bool>(type: "INTEGER", nullable: false),
                    BaseCalendarId = table.Column<string>(type: "TEXT", nullable: true),
                    WorkWeek = table.Column<string>(type: "TEXT", nullable: true),
                    Exceptions = table.Column<string>(type: "TEXT", nullable: true),
                    BaselineId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraCalendars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodeType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CodeTypeId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CodeValue = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CodeName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ParentCodeId = table.Column<string>(type: "TEXT", nullable: true),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SourceFileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ImportSessionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    PredTaskId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SuccTaskId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    LagDuration = table.Column<double>(type: "REAL", nullable: false),
                    BaselineId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraRelationships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraRepairActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    IssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TargetEntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetEntityIds = table.Column<string>(type: "TEXT", nullable: true),
                    AppliedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UndoAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraRepairActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraResourceAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Units = table.Column<double>(type: "REAL", nullable: false),
                    PlannedUnits = table.Column<double>(type: "REAL", nullable: false),
                    ActualUnits = table.Column<double>(type: "REAL", nullable: false),
                    CostPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BaselineId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraResourceAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraUdfs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    UdfTypeId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FieldType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraUdfs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraValidationIssues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    RuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SuggestedFix = table.Column<string>(type: "TEXT", nullable: true),
                    IsResolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraValidationIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrimaveraValidationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaveraValidationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XerExportProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IncludeRawTables = table.Column<bool>(type: "INTEGER", nullable: false),
                    SelectedEntityTypes = table.Column<string>(type: "TEXT", nullable: true),
                    OutputPathTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XerExportProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XerImportSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SourceFileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SourceFileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImportedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RowCounts = table.Column<string>(type: "TEXT", nullable: true),
                    ValidationSummary = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XerImportSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "XerRawTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ColumnHeaders = table.Column<string>(type: "TEXT", nullable: true),
                    Rows = table.Column<string>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XerRawTables", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraActivities_ProjectId",
                table: "PrimaveraActivities",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraActivities_TaskId",
                table: "PrimaveraActivities",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraBaselines_ProjectId_BaselineId_VersionNumber",
                table: "PrimaveraBaselines",
                columns: new[] { "ProjectId", "BaselineId", "VersionNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraCalendars_ProjectId",
                table: "PrimaveraCalendars",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraCodes_ProjectId",
                table: "PrimaveraCodes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraProjects_ProjectId",
                table: "PrimaveraProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraRelationships_ProjectId",
                table: "PrimaveraRelationships",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraRepairActions_ProjectId",
                table: "PrimaveraRepairActions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraResourceAssignments_ProjectId",
                table: "PrimaveraResourceAssignments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraUdfs_ProjectId",
                table: "PrimaveraUdfs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraValidationIssues_ProjectId",
                table: "PrimaveraValidationIssues",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaveraValidationIssues_RuleId",
                table: "PrimaveraValidationIssues",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_XerImportSessions_SourceFileHash_ImportedAt",
                table: "XerImportSessions",
                columns: new[] { "SourceFileHash", "ImportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_XerRawTables_ImportSessionId",
                table: "XerRawTables",
                column: "ImportSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrimaveraActivities");

            migrationBuilder.DropTable(
                name: "PrimaveraBaselines");

            migrationBuilder.DropTable(
                name: "PrimaveraCalendars");

            migrationBuilder.DropTable(
                name: "PrimaveraCodes");

            migrationBuilder.DropTable(
                name: "PrimaveraProjects");

            migrationBuilder.DropTable(
                name: "PrimaveraRelationships");

            migrationBuilder.DropTable(
                name: "PrimaveraRepairActions");

            migrationBuilder.DropTable(
                name: "PrimaveraResourceAssignments");

            migrationBuilder.DropTable(
                name: "PrimaveraUdfs");

            migrationBuilder.DropTable(
                name: "PrimaveraValidationIssues");

            migrationBuilder.DropTable(
                name: "PrimaveraValidationRules");

            migrationBuilder.DropTable(
                name: "XerExportProfiles");

            migrationBuilder.DropTable(
                name: "XerImportSessions");

            migrationBuilder.DropTable(
                name: "XerRawTables");
        }
    }
}
