using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWbsAndBoqEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wbs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Revision = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SourceBoqId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TotalWeight = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    ItemCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wbs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WbsTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Industry = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsStandard = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WbsTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WbsItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WbsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ShortCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    WbsLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SourceBoqItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Weight = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    PlannedStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlannedFinish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationDays = table.Column<int>(type: "INTEGER", nullable: true),
                    AssignedTo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Deliverable = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WbsItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WbsItems_WbsItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "WbsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WbsItems_Wbs_WbsId",
                        column: x => x.WbsId,
                        principalTable: "Wbs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WbsTemplateItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ShortCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    WbsLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DefaultDurationDays = table.Column<int>(type: "INTEGER", nullable: true),
                    TypicalWeight = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WbsTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WbsTemplateItems_WbsTemplateItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "WbsTemplateItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WbsTemplateItems_WbsTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "WbsTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wbs_ProjectId",
                table: "Wbs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Wbs_Status",
                table: "Wbs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WbsItems_ParentId",
                table: "WbsItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_WbsItems_SourceBoqItemId",
                table: "WbsItems",
                column: "SourceBoqItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WbsItems_WbsId",
                table: "WbsItems",
                column: "WbsId");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTemplateItems_ParentId",
                table: "WbsTemplateItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTemplateItems_TemplateId",
                table: "WbsTemplateItems",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WbsItems");

            migrationBuilder.DropTable(
                name: "WbsTemplateItems");

            migrationBuilder.DropTable(
                name: "Wbs");

            migrationBuilder.DropTable(
                name: "WbsTemplates");
        }
    }
}
