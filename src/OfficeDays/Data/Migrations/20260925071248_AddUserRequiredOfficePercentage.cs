using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfficeDays.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRequiredOfficePercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredOfficePercentage",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 50);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_RequiredOfficePercentage",
                table: "Users",
                sql: "\"RequiredOfficePercentage\" BETWEEN 0 AND 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_RequiredOfficePercentage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RequiredOfficePercentage",
                table: "Users");
        }
    }
}
