using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planova.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToWbs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Wbs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Wbs");
        }
    }
}
