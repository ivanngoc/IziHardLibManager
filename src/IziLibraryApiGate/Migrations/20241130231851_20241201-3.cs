using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IziLibraryApiGate.Migrations
{
    /// <inheritdoc />
    public partial class _202412013 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_AsmdefsAtDevice_Devices_DeviceId",
                table: "AsmdefsAtDevice",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelationAsmdefsAtDevice_Devices_DeviceId",
                table: "RelationAsmdefsAtDevice",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsmdefsAtDevice_Devices_DeviceId",
                table: "AsmdefsAtDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_RelationAsmdefsAtDevice_Devices_DeviceId",
                table: "RelationAsmdefsAtDevice");
        }
    }
}
