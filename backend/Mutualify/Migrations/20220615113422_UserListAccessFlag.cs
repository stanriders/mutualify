using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mutualify.Migrations
{
    public partial class UserListAccessFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowsFriendlistAccess",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowsFriendlistAccess",
                table: "Users");
        }
    }
}
