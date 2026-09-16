using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OfficeDays.Data;

namespace OfficeDays.IntegrationTests;

public sealed class MigrationIntegrationTests
{
    [Fact]
    public async Task Jurisdiction_migration_preserves_existing_users_and_holidays_as_Romania()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"officedays-migration-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
                      .UseSqlite($"Data Source={databasePath}")
                      .Options;

        try
        {
            await using var db = new AppDbContext(options);
            var migrator = db.Database.GetService<IMigrator>();
            await migrator.MigrateAsync("20260915195041_VacationDateRangeConstraint");

            var userId = Guid.NewGuid();
            var holidayId = Guid.NewGuid();
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO "Users"
                    ("Id", "Username", "NormalizedUsername", "PasswordHash", "TimeZoneId", "IsAdmin", "CreatedAt")
                VALUES ({0}, 'existing', 'EXISTING', 'hash', 'Europe/Bucharest', 0, '2026-01-01 00:00:00+00:00');
                """, userId);
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO "BankHolidays" ("Id", "Date", "Name")
                VALUES ({0}, '2026-12-01', 'National Day');
                """, holidayId);

            await migrator.MigrateAsync();

            var jurisdiction = await db.HolidayJurisdictions.SingleAsync();
            Assert.Equal("RO", jurisdiction.Code);
            Assert.Equal(jurisdiction.Id, (await db.Users.SingleAsync()).HolidayJurisdictionId);
            Assert.Equal(jurisdiction.Id, (await db.BankHolidays.SingleAsync()).HolidayJurisdictionId);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(databasePath)) File.Delete(databasePath);
        }
    }
}
