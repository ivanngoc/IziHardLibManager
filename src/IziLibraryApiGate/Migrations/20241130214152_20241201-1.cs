using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IziLibraryApiGate.Migrations
{
    /// <inheritdoc />
    public partial class _202412011 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MetaId",
                table: "Asmdefs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Metas",
                columns: table => new
                {
                    MetaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metas", x => x.MetaId);
                });

            migrationBuilder.CreateTable(
                name: "MetasAtDevice",
                columns: table => new
                {
                    MetaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetasAtDevice", x => new { x.DeviceId, x.MetaId });
                    table.ForeignKey(
                        name: "FK_MetasAtDevice_Metas_MetaId",
                        column: x => x.MetaId,
                        principalTable: "Metas",
                        principalColumn: "MetaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asmdefs_MetaId",
                table: "Asmdefs",
                column: "MetaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetasAtDevice_MetaId",
                table: "MetasAtDevice",
                column: "MetaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Asmdefs_Metas_MetaId",
                table: "Asmdefs",
                column: "MetaId",
                principalTable: "Metas",
                principalColumn: "MetaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asmdefs_Metas_MetaId",
                table: "Asmdefs");

            migrationBuilder.DropTable(
                name: "MetasAtDevice");

            migrationBuilder.DropTable(
                name: "Metas");

            migrationBuilder.DropIndex(
                name: "IX_Asmdefs_MetaId",
                table: "Asmdefs");

            migrationBuilder.DropColumn(
                name: "MetaId",
                table: "Asmdefs");
        }
    }
}
