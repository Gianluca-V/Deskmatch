using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskMatch.CoreService.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, object>>(
                name: "DynamicAttributes",
                schema: "core",
                table: "Workspaces",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "core",
                table: "Companies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DynamicAttributes",
                schema: "core",
                table: "Workspaces");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "core",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
