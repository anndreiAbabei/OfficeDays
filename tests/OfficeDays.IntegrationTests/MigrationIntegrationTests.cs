using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OfficeDays.Data;

namespace OfficeDays.IntegrationTests;

public sealed class MigrationIntegrationTests
{
    [Fact]
    public async Task Migrations_preserve_existing_data_and_backfill_only_historical_attendance_as_manual()
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

            var attendanceId = Guid.NewGuid();
            await db.Database.ExecuteSqlRawAsync("""
                INSERT INTO "Attendances" ("Id", "UserId", "Date", "CreatedAt")
                VALUES ({0}, {1}, '2026-09-15', '2026-09-15 09:00:00+00:00');
                """, attendanceId, userId);

            await migrator.MigrateAsync();
            var existingAttendance = await db.Attendances.AsNoTracking().SingleAsync();
            Assert.Equal(attendanceId, existingAttendance.Id);
            Assert.Equal(userId, existingAttendance.UserId);
            Assert.Equal(new DateOnly(2026, 9, 15), existingAttendance.Date);
            Assert.True(existingAttendance.IsManual);

            db.Attendances.Add(new OfficeDays.Domain.Attendance
            {
                Id = Guid.NewGuid(), UserId = userId, Date = new DateOnly(2026, 9, 16),
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
            Assert.False((await db.Attendances.AsNoTracking().SingleAsync(x => x.Date == new DateOnly(2026, 9, 16))).IsManual);
            await migrator.MigrateAsync();
            Assert.False((await db.Attendances.AsNoTracking().SingleAsync(x => x.Date == new DateOnly(2026, 9, 16))).IsManual);


            Assert.Equal(50, (await db.Users.SingleAsync()).RequiredOfficePercentage);
            Assert.Null((await db.Users.SingleAsync()).Email);
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
