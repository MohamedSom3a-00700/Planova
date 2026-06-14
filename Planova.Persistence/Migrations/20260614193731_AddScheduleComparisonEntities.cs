using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleComparisonEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComparisonRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SeverityThresholdCritical = table.Column<double>(type: "REAL", nullable: false),
                    SeverityThresholdMajor = table.Column<double>(type: "REAL", nullable: false),
                    SeverityThresholdMinor = table.Column<double>(type: "REAL", nullable: false),
                    EnableFuzzyMatching = table.Column<bool>(type: "INTEGER", nullable: false),
                    MatchingStrategyPreference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComparisonSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SourceKind = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SourceSnapshotId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourcePrimaveraProjectId = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceLabel = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SourceCapturedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TargetKind = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TargetSnapshotId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TargetPrimaveraProjectId = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetLabel = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TargetCapturedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IncludedScopes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ResultJson = table.Column<string>(type: "TEXT", nullable: true),
                    Error = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BaselineId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SnapshotData = table.Column<string>(type: "TEXT", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActivityCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RelationshipCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComparisonResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    MatchKey = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ChangeType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MatchConfidence = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    NewValue = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComparisonResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComparisonResults_ComparisonSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ComparisonSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonResults_SessionId_EntityType",
                table: "ComparisonResults",
                columns: new[] { "SessionId", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonRules_ProjectId",
                table: "ComparisonRules",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ComparisonSessions_ProjectId",
                table: "ComparisonSessions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSnapshots_ProjectId",
                table: "ScheduleSnapshots",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComparisonResults");

            migrationBuilder.DropTable(
                name: "ComparisonRules");

            migrationBuilder.DropTable(
                name: "ScheduleSnapshots");

            migrationBuilder.DropTable(
                name: "ComparisonSessions");
        }
    }
}
