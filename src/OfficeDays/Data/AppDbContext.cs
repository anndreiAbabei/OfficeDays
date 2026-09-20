using Microsoft.EntityFrameworkCore;
using OfficeDays.Domain;

namespace OfficeDays.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
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

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.Date }).IsUnique();
            entity.HasOne(x => x.User).WithMany(x => x.Attendances).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
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
