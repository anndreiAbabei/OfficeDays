using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OfficeDays.Data.Migrations
{
    /// <inheritdoc />
    public partial class VacationDateRangeConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Vacations_DateRange",
                table: "Vacations",
                sql: "\"To\" >= \"From\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vacations_DateRange",
                table: "Vacations");
        }
    }
}
