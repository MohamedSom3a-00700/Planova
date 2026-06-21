using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultantEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsultantId",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Consultants",
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
                    table.PrimaryKey("PK_Consultants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ConsultantId",
                table: "Projects",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultants_Code",
                table: "Consultants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultants_Name",
                table: "Consultants",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultants_UpdatedAt",
                table: "Consultants",
                column: "UpdatedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Consultants_ConsultantId",
                table: "Projects",
                column: "ConsultantId",
                principalTable: "Consultants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Consultants_ConsultantId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "Consultants");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ConsultantId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ConsultantId",
                table: "Projects");
        }
    }
}
