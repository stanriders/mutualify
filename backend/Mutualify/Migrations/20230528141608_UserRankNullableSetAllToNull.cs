using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mutualify.Migrations
{
    /// <inheritdoc />
    public partial class UserRankNullableSetAllToNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""Users"" SET ""Rank"" = NULL WHERE ""Rank"" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
