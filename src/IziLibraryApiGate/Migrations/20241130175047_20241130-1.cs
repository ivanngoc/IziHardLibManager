using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IziLibraryApiGate.Migrations
{
    /// <inheritdoc />
    public partial class _202411301 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Asmdefs",
                columns: table => new
                {
                    EntityAsmdefId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asmdefs", x => x.EntityAsmdefId);
                });

            migrationBuilder.CreateTable(
                name: "Csprojs",
                columns: table => new
                {
                    EntityCsprojId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountReferencesToProjects = table.Column<int>(type: "integer", nullable: false),
                    RepoGitHub = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Csprojs", x => x.EntityCsprojId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDirs = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AsmdefsAtDevice",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsmdefId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsmdefsAtDevice", x => new { x.DeviceId, x.AsmdefId });
                    table.ForeignKey(
                        name: "FK_AsmdefsAtDevice_Asmdefs_AsmdefId",
                        column: x => x.AsmdefId,
                        principalTable: "Asmdefs",
                        principalColumn: "EntityAsmdefId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelationAsmdefs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RelationType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationAsmdefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelationAsmdefs_Asmdefs_FromId",
                        column: x => x.FromId,
                        principalTable: "Asmdefs",
                        principalColumn: "EntityAsmdefId");
                    table.ForeignKey(
                        name: "FK_RelationAsmdefs_Asmdefs_ToId",
                        column: x => x.ToId,
                        principalTable: "Asmdefs",
                        principalColumn: "EntityAsmdefId");
                });

            migrationBuilder.CreateTable(
                name: "AsmdefXCsproj",
                columns: table => new
                {
                    CsprojId = table.Column<Guid>(type: "uuid", nullable: false),
                    AsmdefId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsmdefXCsproj", x => new { x.AsmdefId, x.CsprojId });
                    table.ForeignKey(
                        name: "FK_AsmdefXCsproj_Asmdefs_AsmdefId",
                        column: x => x.AsmdefId,
                        principalTable: "Asmdefs",
                        principalColumn: "EntityAsmdefId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsmdefXCsproj_Csprojs_CsprojId",
                        column: x => x.CsprojId,
                        principalTable: "Csprojs",
                        principalColumn: "EntityCsprojId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Relations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChildId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    CheckDateTime = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relations_Csprojs_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Csprojs",
                        principalColumn: "EntityCsprojId");
                    table.ForeignKey(
                        name: "FK_Relations_Csprojs_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Csprojs",
                        principalColumn: "EntityCsprojId");
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DeviceSettingsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_DeviceSettings_Id",
                        column: x => x.Id,
                        principalTable: "DeviceSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelationAsmdefsAtDevice",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationAsmdefsAtDevice", x => new { x.DeviceId, x.RelationId });
                    table.ForeignKey(
                        name: "FK_RelationAsmdefsAtDevice_RelationAsmdefs_RelationId",
                        column: x => x.RelationId,
                        principalTable: "RelationAsmdefs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceIziProject",
                columns: table => new
                {
                    DevicesId = table.Column<Guid>(type: "uuid", nullable: false),
                    IziProjectsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceIziProject", x => new { x.DevicesId, x.IziProjectsId });
                    table.ForeignKey(
                        name: "FK_DeviceIziProject_Devices_DevicesId",
                        column: x => x.DevicesId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceIziProject_Projects_IziProjectsId",
                        column: x => x.IziProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectsAtDevice",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityCsprojId = table.Column<Guid>(type: "uuid", nullable: false),
                    PathAbs = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectsAtDevice", x => new { x.DeviceId, x.EntityCsprojId });
                    table.ForeignKey(
                        name: "FK_ProjectsAtDevice_Csprojs_EntityCsprojId",
                        column: x => x.EntityCsprojId,
                        principalTable: "Csprojs",
                        principalColumn: "EntityCsprojId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectsAtDevice_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelationsAtDevice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RelationId = table.Column<int>(type: "integer", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Include = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationsAtDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelationsAtDevice_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelationsAtDevice_Relations_RelationId",
                        column: x => x.RelationId,
                        principalTable: "Relations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsmdefsAtDevice_AsmdefId",
                table: "AsmdefsAtDevice",
                column: "AsmdefId");

            migrationBuilder.CreateIndex(
                name: "IX_AsmdefXCsproj_CsprojId",
                table: "AsmdefXCsproj",
                column: "CsprojId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIziProject_IziProjectsId",
                table: "DeviceIziProject",
                column: "IziProjectsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectsAtDevice_EntityCsprojId",
                table: "ProjectsAtDevice",
                column: "EntityCsprojId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectsAtDevice_PathAbs",
                table: "ProjectsAtDevice",
                column: "PathAbs",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelationAsmdefs_FromId_ToId",
                table: "RelationAsmdefs",
                columns: new[] { "FromId", "ToId" },
                unique: true,
                filter: "\"FromId\" IS NOT NULL AND \"ToId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RelationAsmdefs_ToId",
                table: "RelationAsmdefs",
                column: "ToId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationAsmdefsAtDevice_RelationId",
                table: "RelationAsmdefsAtDevice",
                column: "RelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Relations_ChildId",
                table: "Relations",
                column: "ChildId");

            migrationBuilder.CreateIndex(
                name: "IX_Relations_ParentId_ChildId",
                table: "Relations",
                columns: new[] { "ParentId", "ChildId" },
                unique: true,
                filter: "\"ParentId\" IS NOT NULL AND \"ChildId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RelationsAtDevice_DeviceId",
                table: "RelationsAtDevice",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationsAtDevice_Include",
                table: "RelationsAtDevice",
                column: "Include");

            migrationBuilder.CreateIndex(
                name: "IX_RelationsAtDevice_RelationId_DeviceId",
                table: "RelationsAtDevice",
                columns: new[] { "RelationId", "DeviceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsmdefsAtDevice");

            migrationBuilder.DropTable(
                name: "AsmdefXCsproj");

            migrationBuilder.DropTable(
                name: "DeviceIziProject");

            migrationBuilder.DropTable(
                name: "ProjectsAtDevice");

            migrationBuilder.DropTable(
                name: "RelationAsmdefsAtDevice");

            migrationBuilder.DropTable(
                name: "RelationsAtDevice");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "RelationAsmdefs");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Relations");

            migrationBuilder.DropTable(
                name: "Asmdefs");

            migrationBuilder.DropTable(
                name: "DeviceSettings");

            migrationBuilder.DropTable(
                name: "Csprojs");
        }
    }
}
