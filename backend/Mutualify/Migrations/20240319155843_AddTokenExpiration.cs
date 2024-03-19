using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mutualify.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresOn",
                table: "Tokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: DateTime.UtcNow.AddDays(1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresOn",
                table: "Tokens");
        }
    }
}
