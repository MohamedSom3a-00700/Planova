using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContractorSubcontractorEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContractorId",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubcontractorId",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Clients",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contractors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    OrganizationDetails = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Logo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contractors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subcontractors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    OrganizationDetails = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Trade = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LicenseNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Logo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcontractors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ContractorId",
                table: "Projects",
                column: "ContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SubcontractorId",
                table: "Projects",
                column: "SubcontractorId");

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_Code",
                table: "Contractors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_Name",
                table: "Contractors",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contractors_UpdatedAt",
                table: "Contractors",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_Code",
                table: "Subcontractors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_Name",
                table: "Subcontractors",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subcontractors_UpdatedAt",
                table: "Subcontractors",
                column: "UpdatedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Contractors_ContractorId",
                table: "Projects",
                column: "ContractorId",
                principalTable: "Contractors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Subcontractors_SubcontractorId",
                table: "Projects",
                column: "SubcontractorId",
                principalTable: "Subcontractors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Contractors_ContractorId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Subcontractors_SubcontractorId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "Contractors");

            migrationBuilder.DropTable(
                name: "Subcontractors");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ContractorId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SubcontractorId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ContractorId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SubcontractorId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Clients");
        }
    }
}
