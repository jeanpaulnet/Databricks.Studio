using Databricks.Studio.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Databricks.Studio.Entity.Data;

public class StudioDbContext : DbContext
{
    public StudioDbContext(DbContextOptions<StudioDbContext> options) : base(options) { }

    public DbSet<AnalyticsEntity> Analytics => Set<AnalyticsEntity>();
    public DbSet<AnalyticsRunEntity> AnalyticsRuns => Set<AnalyticsRunEntity>();
    public DbSet<HistoryEntity> History => Set<HistoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnalyticsEntity>(e =>
        {
            e.ToTable("Analytics");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Status).IsRequired();
        });

        modelBuilder.Entity<AnalyticsRunEntity>(e =>
        {
            e.ToTable("AnalyticsRuns");
            e.HasKey(x => x.Id);
            e.Property(x => x.JobId).IsRequired().HasMaxLength(256);
            e.Property(x => x.InputJson).HasMaxLength(4000);
            e.Property(x => x.OutputJson).HasMaxLength(4000);
            e.Property(x => x.Status).IsRequired();
            e.Property(x => x.StartedOn).IsRequired();
            e.HasOne(x => x.Analytics)
             .WithMany(x => x.Runs)
             .HasForeignKey(x => x.AnalyticsId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HistoryEntity>(e =>
        {
            e.ToTable("History");
            e.HasKey(x => x.Id);
            e.Property(x => x.EntityType).IsRequired().HasMaxLength(256);
            e.Property(x => x.EntityJson).IsRequired();
            e.Property(x => x.ActionType).IsRequired().HasMaxLength(50);
            e.Property(x => x.ActionBy).IsRequired().HasMaxLength(256);
            e.Property(x => x.ActionOn).IsRequired();
        });
    }
}
