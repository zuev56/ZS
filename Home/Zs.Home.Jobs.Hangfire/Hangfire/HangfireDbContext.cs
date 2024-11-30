using Microsoft.EntityFrameworkCore;
using static Zs.Home.Jobs.Hangfire.Constants;

namespace Zs.Home.Jobs.Hangfire.Hangfire;

// dotnet ef dbcontext scaffold "My connection string" Npgsql.EntityFrameworkCore.PostgreSQL --table hangfire.lock
public partial class HangfireDbContext : DbContext
{
    public HangfireDbContext(DbContextOptions<HangfireDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Lock> Locks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lock>(entity =>
        {
            entity.HasNoKey()
                .ToTable("lock", HangfireSchema);

            entity.Property(e => e.Acquired)
                .HasColumnName("acquired");
            entity.Property(e => e.Resource)
                .HasColumnName("resource");
            entity.Property(e => e.Updatecount)
                .HasDefaultValue(0)
                .HasColumnName("updatecount");
        });
    }
}
