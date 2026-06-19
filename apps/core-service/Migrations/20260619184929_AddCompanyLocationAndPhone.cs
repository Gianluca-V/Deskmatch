using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskMatch.CoreService.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyLocationAndPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "core",
                table: "Companies",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "core",
                table: "Companies",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                schema: "core",
                table: "Companies",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                schema: "core",
                table: "Companies",
                type: "double precision",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                schema: "core",
                table: "Companies",
                type: "double precision",
                precision: 10,
                scale: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "core",
                table: "Companies",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "core",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "core",
                table: "Companies");
        }
    }
}
