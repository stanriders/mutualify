using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mutualify.Migrations
{
    /// <inheritdoc />
    public partial class UserAddRank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Rank",
                table: "Users",
                column: "Rank");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Rank",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Users");
        }
    }
}
