using Microsoft.EntityFrameworkCore;
using OfficeDays.Domain;
using OfficeDays.Infrastructure;

namespace OfficeDays.Data;

public interface IAppDbContext : IDbContext
{
    DbSet<OfficePlan> OfficePlans { get; }
    DbSet<User> Users { get; }
    DbSet<UserAttendanceRequirement> UserAttendanceRequirements { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<Vacation> Vacations { get; }
    DbSet<ApiToken> ApiTokens { get; }
    DbSet<BankHoliday> BankHolidays { get; }
    DbSet<HolidayJurisdiction> HolidayJurisdictions { get; }
}
public sealed class AppDbContext : DbContext, IAppDbContext
{
    public DbSet<OfficePlan> OfficePlans => Set<OfficePlan>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserAttendanceRequirement> UserAttendanceRequirements => Set<UserAttendanceRequirement>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Vacation> Vacations => Set<Vacation>();
    public DbSet<ApiToken> ApiTokens => Set<ApiToken>();
    public DbSet<BankHoliday> BankHolidays => Set<BankHoliday>();
    public DbSet<HolidayJurisdiction> HolidayJurisdictions => Set<HolidayJurisdiction>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.ComplexProperty(x => x.Preferences, preferences => preferences.ToJson());
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.NormalizedUsername).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.NormalizedUsername).IsUnique();
            entity.HasOne(x => x.HolidayJurisdiction)
                  .WithMany(x => x.Users)
                  .HasForeignKey(x => x.HolidayJurisdictionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserAttendanceRequirement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Ignore(x => x.EffectivePercentage);
            entity.Property(x => x.RequirementMode).HasConversion<string>();
            entity.HasIndex(x => new { x.UserId, x.StartingFrom }).IsUnique();
            entity.HasOne<User>().WithMany(x => x.AttendanceRequirements)
                  .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_UserAttendanceRequirements_Value", "(\"RequirementMode\" = 'Percentage' AND \"RequirementValue\" BETWEEN 0 AND 100) OR (\"RequirementMode\" = 'DaysPerWeek' AND \"RequirementValue\" BETWEEN 0 AND 5)");
                table.HasCheckConstraint("CK_UserAttendanceRequirements_Month", "substr(\"StartingFrom\", 9, 2) = '01'");
            });
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.Attendances).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OfficePlan>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vacation>(entity =>
        {
            entity.ToTable(table => table.HasCheckConstraint("CK_Vacations_DateRange", "\"To\" >= \"From\""));
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.From, x.To });
            entity.HasOne(x => x.User).WithMany(x => x.Vacations).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.RevokedAt });
            entity.HasOne(x => x.User).WithMany(x => x.ApiTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BankHoliday>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => new { x.HolidayJurisdictionId, x.Date }).IsUnique();
            entity.HasOne(x => x.HolidayJurisdiction)
                  .WithMany(x => x.BankHolidays)
                  .HasForeignKey(x => x.HolidayJurisdictionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HolidayJurisdiction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(6).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();
        });
    }
}
