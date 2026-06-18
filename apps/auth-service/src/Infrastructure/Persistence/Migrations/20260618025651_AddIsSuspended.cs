using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeskMatch.AuthService.src.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSuspended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "AspNetUsers");
        }
    }
}
