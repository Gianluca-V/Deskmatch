using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskMatch.CoreService.Migrations
{
    /// <inheritdoc />
    public partial class AddKybAndAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KybRejectionReason",
                schema: "core",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "KybReviewedAt",
                schema: "core",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KybReviewedByAdminId",
                schema: "core",
                table: "Companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KybStatus",
                schema: "core",
                table: "Companies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "KybSubmittedAt",
                schema: "core",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                schema: "core",
                table: "Companies",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationDocumentUrl",
                schema: "core",
                table: "Companies",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                schema: "core",
                table: "Companies",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "KybRejectionReason",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "KybReviewedAt",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "KybReviewedByAdminId",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "KybStatus",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "KybSubmittedAt",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LegalName",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RegistrationDocumentUrl",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TaxId",
                schema: "core",
                table: "Companies");
        }
    }
}
