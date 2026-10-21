using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfficeDays.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceRequirementModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAttendanceRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartingFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttendanceRequirements", x => x.Id);
                    table.CheckConstraint("CK_UserAttendanceRequirements_Month", "substr(\"StartingFrom\", 9, 2) = '01'");
                    table.CheckConstraint("CK_UserAttendanceRequirements_Value", "(\"Mode\" = 'Percentage' AND \"Value\" BETWEEN 0 AND 100) OR (\"Mode\" = 'DaysPerWeek' AND \"Value\" BETWEEN 0 AND 5)");
                    table.ForeignKey(
                        name: "FK_UserAttendanceRequirements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Copy before dropping so identities, effective months and values survive the migration.
            migrationBuilder.Sql("""
                INSERT INTO "UserAttendanceRequirements" ("Id", "UserId", "StartingFrom", "Mode", "Value", "CreatedAt")
                SELECT "Id", "UserId", "StartingFrom", 'Percentage', "RequiredOfficePercentage", "CreatedAt" FROM "UserRequiredOfficePercentages";
                """);

            migrationBuilder.DropTable(name: "UserRequiredOfficePercentages");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttendanceRequirements_UserId_StartingFrom",
                table: "UserAttendanceRequirements",
                columns: new[] { "UserId", "StartingFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRequiredOfficePercentages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    RequiredOfficePercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    StartingFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
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

            // Copy before dropping so identities, effective months and values survive the migration.
            migrationBuilder.Sql("""
                INSERT INTO "UserRequiredOfficePercentages" ("Id", "UserId", "StartingFrom", "RequiredOfficePercentage", "CreatedAt")
                SELECT "Id", "UserId", "StartingFrom", CASE WHEN "Mode" = 'DaysPerWeek' THEN "Value" * 20 ELSE "Value" END, "CreatedAt" FROM "UserAttendanceRequirements";
                """);

            migrationBuilder.DropTable(name: "UserAttendanceRequirements");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequiredOfficePercentages_UserId_StartingFrom",
                table: "UserRequiredOfficePercentages",
                columns: new[] { "UserId", "StartingFrom" },
                unique: true);
        }
    }
}
