using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfficeDays.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameAttendanceRequirementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAttendanceRequirements_Value",
                table: "UserAttendanceRequirements");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "UserAttendanceRequirements",
                newName: "RequirementValue");

            migrationBuilder.RenameColumn(
                name: "Mode",
                table: "UserAttendanceRequirements",
                newName: "RequirementMode");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAttendanceRequirements_Value",
                table: "UserAttendanceRequirements",
                sql: "(\"RequirementMode\" = 'Percentage' AND \"RequirementValue\" BETWEEN 0 AND 100) OR (\"RequirementMode\" = 'DaysPerWeek' AND \"RequirementValue\" BETWEEN 0 AND 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAttendanceRequirements_Value",
                table: "UserAttendanceRequirements");

            migrationBuilder.RenameColumn(
                name: "RequirementValue",
                table: "UserAttendanceRequirements",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "RequirementMode",
                table: "UserAttendanceRequirements",
                newName: "Mode");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAttendanceRequirements_Value",
                table: "UserAttendanceRequirements",
                sql: "(\"Mode\" = 'Percentage' AND \"Value\" BETWEEN 0 AND 100) OR (\"Mode\" = 'DaysPerWeek' AND \"Value\" BETWEEN 0 AND 5)");
        }
    }
}
