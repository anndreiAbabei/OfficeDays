using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfficeDays.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficeRequirementHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRequiredOfficePercentages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartingFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    RequiredOfficePercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequiredOfficePercentages", x => x.Id);
                    table.CheckConstraint("CK_UserRequiredOfficePercentages_Month", "substr(\"StartingFrom\", 9, 2) = '01'");
                    table.CheckConstraint("CK_UserRequiredOfficePercentages_Percentage", "\"RequiredOfficePercentage\" BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_UserRequiredOfficePercentages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRequiredOfficePercentages_UserId_StartingFrom",
                table: "UserRequiredOfficePercentages",
                columns: new[] { "UserId", "StartingFrom" },
                unique: true);

            // Preserve each existing value before removing the legacy column. The sentinel
            // baseline date deliberately covers attendance recorded before registration.
            migrationBuilder.Sql("""
                INSERT INTO "UserRequiredOfficePercentages"
                    ("Id", "UserId", "StartingFrom", "RequiredOfficePercentage", "CreatedAt")
                SELECT upper(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-' ||
                    '4' || substr(hex(randomblob(2)), 2) || '-' ||
                    '8' || substr(hex(randomblob(2)), 2) || '-' || hex(randomblob(6))),
                    "Id", '0001-01-01', "RequiredOfficePercentage", strftime('%Y-%m-%d %H:%M:%f+00:00', 'now')
                FROM "Users";
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_RequiredOfficePercentage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RequiredOfficePercentage",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredOfficePercentage",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 50);

            // Downgrades preserve the baseline; the old schema cannot represent a schedule.
            migrationBuilder.Sql("""
                UPDATE "Users" SET "RequiredOfficePercentage" = (
                    SELECT "RequiredOfficePercentage" FROM "UserRequiredOfficePercentages"
                    WHERE "UserId" = "Users"."Id" AND "StartingFrom" = '0001-01-01');
                """);
            migrationBuilder.DropTable(name: "UserRequiredOfficePercentages");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_RequiredOfficePercentage",
                table: "Users",
                sql: "\"RequiredOfficePercentage\" BETWEEN 0 AND 100");
        }
    }
}
