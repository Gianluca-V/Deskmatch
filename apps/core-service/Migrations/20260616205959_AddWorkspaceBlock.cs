using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskMatch.CoreService.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkspaceSchedules_WorkspaceId",
                schema: "core",
                table: "WorkspaceSchedules");

            migrationBuilder.CreateTable(
                name: "WorkspaceBlocks",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BlockEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceBlocks", x => x.Id);
                    table.CheckConstraint("CK_WorkspaceBlock_Times", "\"BlockEnd\" > \"BlockStart\"");
                    table.ForeignKey(
                        name: "FK_WorkspaceBlocks_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalSchema: "core",
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceSchedules_WorkspaceId_DayOfWeek",
                schema: "core",
                table: "WorkspaceSchedules",
                columns: new[] { "WorkspaceId", "DayOfWeek" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_WorkspaceSchedule_Times",
                schema: "core",
                table: "WorkspaceSchedules",
                sql: "\"CloseTime\" > \"OpenTime\"");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceBlocks_WorkspaceId",
                schema: "core",
                table: "WorkspaceBlocks",
                column: "WorkspaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceBlocks",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceSchedules_WorkspaceId_DayOfWeek",
                schema: "core",
                table: "WorkspaceSchedules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WorkspaceSchedule_Times",
                schema: "core",
                table: "WorkspaceSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceSchedules_WorkspaceId",
                schema: "core",
                table: "WorkspaceSchedules",
                column: "WorkspaceId");
        }
    }
}
