using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OverallEnhancementPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discipline",
                table: "WbsItems",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "WbsItems",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActivityCount",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualCost",
                table: "Projects",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Projects",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectedDatabase",
                table: "Projects",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectedXerPath",
                table: "Projects",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostHealthPct",
                table: "Projects",
                type: "TEXT",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePath",
                table: "Projects",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cpi",
                table: "Projects",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CriticalActivityCount",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBudget",
                table: "Projects",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CurrentDataDate",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EarnedValue",
                table: "Projects",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleMapsLink",
                table: "Projects",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HealthIndicator",
                table: "Projects",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "Green");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExportDate",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastImportDate",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalBudget",
                table: "Projects",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProgressPercentage",
                table: "Projects",
                type: "TEXT",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectFolderPath",
                table: "Projects",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "Projects",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "Low");

            migrationBuilder.AddColumn<decimal>(
                name: "ScheduleHealthPct",
                table: "Projects",
                type: "TEXT",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Spi",
                table: "Projects",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ImportSessionId",
                table: "PrimaveraProjects",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateTable(
                name: "BoqImportSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    TotalSheetsDetected = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSheetsImported = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSectionsCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalItemsImported = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ImportMode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    ImportedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoqImportSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PartyType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LogoPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WbsEditLocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WbsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LockedByUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LockExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WbsEditLocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WbsEditLocks_Wbs_WbsId",
                        column: x => x.WbsId,
                        principalTable: "Wbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoqWorksheetMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ImportSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorksheetName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchConfidence = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    ColumnMappings = table.Column<string>(type: "TEXT", nullable: false),
                    RowsImported = table.Column<int>(type: "INTEGER", nullable: false),
                    SectionAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoqWorksheetMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoqWorksheetMappings_BoqImportSessions_ImportSessionId",
                        column: x => x.ImportSessionId,
                        principalTable: "BoqImportSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPartyLinks",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPartyLinks", x => new { x.ProjectId, x.PartyId });
                    table.ForeignKey(
                        name: "FK_ProjectPartyLinks_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectPartyLinks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoqWorksheetMappings_ImportSessionId",
                table: "BoqWorksheetMappings",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_Name",
                table: "Parties",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPartyLinks_PartyId",
                table: "ProjectPartyLinks",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPartyLinks_ProjectId_PartyId",
                table: "ProjectPartyLinks",
                columns: new[] { "ProjectId", "PartyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WbsEditLocks_WbsId",
                table: "WbsEditLocks",
                column: "WbsId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoqWorksheetMappings");

            migrationBuilder.DropTable(
                name: "ProjectPartyLinks");

            migrationBuilder.DropTable(
                name: "WbsEditLocks");

            migrationBuilder.DropTable(
                name: "BoqImportSessions");

            migrationBuilder.DropTable(
                name: "Parties");

            migrationBuilder.DropColumn(
                name: "Discipline",
                table: "WbsItems");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "WbsItems");

            migrationBuilder.DropColumn(
                name: "ActivityCount",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ActualCost",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ConnectedDatabase",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ConnectedXerPath",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CostHealthPct",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CoverImagePath",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Cpi",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CriticalActivityCount",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CurrentBudget",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CurrentDataDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "EarnedValue",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "GoogleMapsLink",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "HealthIndicator",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LastExportDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LastImportDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OriginalBudget",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProgressPercentage",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectFolderPath",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ScheduleHealthPct",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Spi",
                table: "Projects");

            migrationBuilder.AlterColumn<int>(
                name: "ImportSessionId",
                table: "PrimaveraProjects",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");
        }
    }
}
