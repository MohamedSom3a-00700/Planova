using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentActivityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    WbsItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CalendarId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ActivityType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: true),
                    PlannedStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlannedFinish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualFinish = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PercentComplete = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_Activities_ParentActivityId",
                        column: x => x.ParentActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityBanks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Subcategory = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsStandard = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityBanks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Calendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HoursPerDay = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    DaysPerWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    Monday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Tuesday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Wednesday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Thursday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Friday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Saturday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sunday = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    PredecessorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SuccessorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    LagDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityRelationships_Activities_PredecessorId",
                        column: x => x.PredecessorId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityRelationships_Activities_SuccessorId",
                        column: x => x.SuccessorId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityBankItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BankId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultActivityType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityBankItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityBankItems_ActivityBankItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ActivityBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityBankItems_ActivityBanks_BankId",
                        column: x => x.BankId,
                        principalTable: "ActivityBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CalendarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarDays_Calendars_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityBankItemRelationships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BankId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PredecessorItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SuccessorItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    DefaultLagDays = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityBankItemRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityBankItemRelationships_ActivityBankItems_PredecessorItemId",
                        column: x => x.PredecessorItemId,
                        principalTable: "ActivityBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityBankItemRelationships_ActivityBankItems_SuccessorItemId",
                        column: x => x.SuccessorItemId,
                        principalTable: "ActivityBankItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityBankItemRelationships_ActivityBanks_BankId",
                        column: x => x.BankId,
                        principalTable: "ActivityBanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CalendarId",
                table: "Activities",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ParentActivityId",
                table: "Activities",
                column: "ParentActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ProjectId",
                table: "Activities",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ProjectId_Code",
                table: "Activities",
                columns: new[] { "ProjectId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Status",
                table: "Activities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_WbsItemId",
                table: "Activities",
                column: "WbsItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBankItemRelationships_BankId",
                table: "ActivityBankItemRelationships",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBankItemRelationships_PredecessorItemId",
                table: "ActivityBankItemRelationships",
                column: "PredecessorItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBankItemRelationships_SuccessorItemId",
                table: "ActivityBankItemRelationships",
                column: "SuccessorItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBankItems_BankId",
                table: "ActivityBankItems",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBankItems_ParentId",
                table: "ActivityBankItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBanks_Category",
                table: "ActivityBanks",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityBanks_Category_Code",
                table: "ActivityBanks",
                columns: new[] { "Category", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRelationships_PredecessorId",
                table: "ActivityRelationships",
                column: "PredecessorId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRelationships_ProjectId",
                table: "ActivityRelationships",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRelationships_SuccessorId",
                table: "ActivityRelationships",
                column: "SuccessorId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarDays_CalendarId_Date",
                table: "CalendarDays",
                columns: new[] { "CalendarId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_ProjectId",
                table: "Calendars",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityBankItemRelationships");

            migrationBuilder.DropTable(
                name: "ActivityRelationships");

            migrationBuilder.DropTable(
                name: "CalendarDays");

            migrationBuilder.DropTable(
                name: "ActivityBankItems");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "Calendars");

            migrationBuilder.DropTable(
                name: "ActivityBanks");
        }
    }
}
