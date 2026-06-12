using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingCenterEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectParties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    LogoPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectParties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EnabledSectionsJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReportType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    LayoutJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GeneratedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DataSnapshotJson = table.Column<string>(type: "TEXT", nullable: false),
                    AiNarrative = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportInstances_ReportTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ReportTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Frequency = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: true),
                    DayOfMonth = table.Column<int>(type: "INTEGER", nullable: true),
                    TimeOfDay = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    TimeZoneId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ExportFormats = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LastErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    LastSuccessfulRunAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxRetries = table.Column<int>(type: "INTEGER", nullable: false),
                    NextRunAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportSchedules_ReportTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ReportTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReportExports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReportInstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Format = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    ExportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExportedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportExports_ReportInstances_ReportInstanceId",
                        column: x => x.ReportInstanceId,
                        principalTable: "ReportInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReportInstanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SectionType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportSections_ReportInstances_ReportInstanceId",
                        column: x => x.ReportInstanceId,
                        principalTable: "ReportInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectParties_ProjectId_Role",
                table: "ProjectParties",
                columns: new[] { "ProjectId", "Role" },
                unique: true,
                filter: "[Role] IN ('Client', 'MainContractor')");

            migrationBuilder.CreateIndex(
                name: "IX_ReportExports_ReportInstanceId",
                table: "ReportExports",
                column: "ReportInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportInstances_ProjectId_ReportType_PeriodStart",
                table: "ReportInstances",
                columns: new[] { "ProjectId", "ReportType", "PeriodStart" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportInstances_ProjectId_Status",
                table: "ReportInstances",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportInstances_TemplateId",
                table: "ReportInstances",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_ProjectId_ReportType",
                table: "ReportSchedules",
                columns: new[] { "ProjectId", "ReportType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportSchedules_TemplateId",
                table: "ReportSchedules",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSections_ReportInstanceId",
                table: "ReportSections",
                column: "ReportInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSettings_ProjectId_ReportType",
                table: "ReportSettings",
                columns: new[] { "ProjectId", "ReportType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_ProjectId_ReportType",
                table: "ReportTemplates",
                columns: new[] { "ProjectId", "ReportType" },
                unique: true,
                filter: "[IsDefault] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectParties");

            migrationBuilder.DropTable(
                name: "ReportExports");

            migrationBuilder.DropTable(
                name: "ReportSchedules");

            migrationBuilder.DropTable(
                name: "ReportSections");

            migrationBuilder.DropTable(
                name: "ReportSettings");

            migrationBuilder.DropTable(
                name: "ReportInstances");

            migrationBuilder.DropTable(
                name: "ReportTemplates");
        }
    }
}
