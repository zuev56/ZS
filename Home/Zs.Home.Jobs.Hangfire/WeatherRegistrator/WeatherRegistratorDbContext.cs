using Microsoft.EntityFrameworkCore;
using Zs.Home.Jobs.Hangfire.WeatherRegistrator.Models;
using static Zs.Home.Jobs.Hangfire.Constants;

namespace Zs.Home.Jobs.Hangfire.WeatherRegistrator;

public sealed class WeatherRegistratorDbContext : DbContext
{
    public WeatherRegistratorDbContext(DbContextOptions<WeatherRegistratorDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.Place> Places { get; set; }
    public DbSet<Source> Sources { get; set; }
    public DbSet<WeatherData> WeatherData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(WeatherRegistratorSchema);
        modelBuilder.UseSerialColumns();

        ConfigureEntities(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Place>(b =>
        {
            b.ToTable("places");

            b.HasKey(e => e.Id);

            b.Property(e => e.Name);
            b.Property(e => e.Description);
            b.Property(e => e.CreatedAt)
                .HasDefaultValueSql(PostgresUtcNowValue)
                .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Source>(b =>
        {
            b.ToTable("sources");

            b.HasKey(e => e.Id);

            b.Property(e => e.PlaceId);
            b.Property(e => e.Name);
            b.Property(e => e.Description);
            b.Property(e => e.CreatedAt)
                .HasDefaultValueSql(PostgresUtcNowValue)
                .ValueGeneratedOnAdd();

            b.HasOne(e => e.Place)
                .WithMany(e => e.Sources)
                .HasForeignKey(e => e.PlaceId);
        });

        modelBuilder.Entity<WeatherData>(b =>
        {
            b.ToTable("weather_data");

            b.HasKey(e => e.Id);

            b.Property(e => e.SourceId);
            b.Property(e => e.CreatedAt)
                .HasDefaultValueSql(PostgresUtcNowValue)
                .ValueGeneratedOnAdd();
            b.Property(e => e.Temperature);
            b.Property(e => e.Humidity);
            b.Property(e => e.Pressure);
            b.Property(e => e.CO2);

            b.HasOne(e => e.Source)
                .WithMany()
                .HasForeignKey(e => e.SourceId);
        });
    }
}
