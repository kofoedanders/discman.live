using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Web.Users;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAchievementsMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "achievements",
                table: "rounds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Achievement>>(
                name: "achievements",
                table: "rounds",
                type: "jsonb",
                nullable: true);
        }
    }
}
