using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IziLibraryApiGate.Migrations
{
    /// <inheritdoc />
    public partial class _202412012 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PathAbs",
                table: "AsmdefsAtDevice",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PathAbs",
                table: "AsmdefsAtDevice");
        }
    }
}
