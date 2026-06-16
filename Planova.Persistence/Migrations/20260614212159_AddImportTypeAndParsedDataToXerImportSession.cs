using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImportTypeAndParsedDataToXerImportSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImportType",
                table: "XerImportSessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParsedDataJson",
                table: "XerImportSessions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportType",
                table: "XerImportSessions");

            migrationBuilder.DropColumn(
                name: "ParsedDataJson",
                table: "XerImportSessions");
        }
    }
}
