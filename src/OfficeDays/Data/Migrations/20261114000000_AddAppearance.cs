using Microsoft.EntityFrameworkCore.Migrations;

namespace OfficeDays.Data.Migrations;

public partial class AddAppearance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""UPDATE Users SET Preferences = json_set(Preferences, '$.Appearance', 'System')""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""UPDATE Users SET Preferences = json_remove(Preferences, '$.Appearance')""");
    }
}
