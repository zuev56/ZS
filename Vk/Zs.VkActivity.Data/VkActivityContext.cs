using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Zs.VkActivity.Data.Models;

namespace Zs.VkActivity.Data;

public partial class VkActivityContext : DbContext
{
    public DbSet<ActivityLogItem>? VkActivityLog { get; set; }
    public DbSet<User>? VkUsers { get; set; }

    public VkActivityContext()
    {
    }

    public VkActivityContext(DbContextOptions<VkActivityContext> options)
       : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        ConfigureEntities(modelBuilder);
    }

    private void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLogItem>(b =>
        {
            b.Property(e => e.Id)
            .HasColumnType("int")
            .HasColumnName("id")
            .ValueGeneratedOnAdd()
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            b.Property(e => e.UserId)
            .HasColumnType("int")
            .HasColumnName("user_id")
            .IsRequired();

            b.Property(e => e.IsOnline)
            .HasColumnType("bool")
            .HasColumnName("is_online");

            b.Property(e => e.Platform)
            .HasColumnType("int")
            .HasColumnName("platform")
            .HasConversion(p => (int)p, p => (Platform)p)
            .IsRequired();

            b.Property(e => e.InsertDate)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("insert_date")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("now()");

            b.Property(e => e.LastSeen)
            .HasColumnType("int")
            .HasColumnName("last_seen");

            b.HasKey(e => e.Id);

            b.HasIndex(e => new { e.UserId, e.LastSeen, e.InsertDate });

            b.ToTable("activity_log", "vk");

            b.HasComment("Vk users activity log item");
        });

        modelBuilder.Entity<User>(b =>
        {
            b.Property(e => e.Id)
            .HasColumnType("int")
            .HasColumnName("id")
            .ValueGeneratedOnAdd()
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            b.Property(e => e.FirstName)
            .HasColumnType("character varying(50)")
            .HasColumnName("first_name")
            .HasMaxLength(50);

            b.Property(e => e.LastName)
            .HasColumnType("character varying(50)")
            .HasColumnName("last_name")
            .HasMaxLength(50);

            b.Property(e => e.Status)
            .HasColumnType("integer")
            .HasColumnName("status");

            b.Property(e => e.RawData)
            .HasColumnType("json")
            .HasColumnName("raw_data");

            b.Property(e => e.RawDataHistory)
            .HasColumnType("json")
            .HasColumnName("raw_data_history");

            b.Property(e => e.InsertDate)
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("insert_date")
                .HasDefaultValueSql("now()");

            b.Property(e => e.UpdateDate)
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("update_date")
                .HasDefaultValueSql("now()");

            b.HasKey(e => e.Id);

            b.ToTable("users", "vk");
        });
    }

    public static string GetOtherSqlScripts()
    {
        var configuration = new ConfigurationBuilder()
               .AddJsonFile(System.IO.Path.GetFullPath("appsettings.json"))
               .Build();

        var connectionStringBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = configuration["ConnectionStrings:Default"]
        };
        var dbName = connectionStringBuilder["Database"] as string;

        var dataDirPath = Directory.GetCurrentDirectory() == "/app" ? "./" : "../Zs.VkActivity.Data/SQL";

        var sqlFilePaths = Directory.GetFiles(dataDirPath, "*.sql", SearchOption.AllDirectories).ToList();

        var sb = new StringBuilder();
        foreach (var sqlFilePath in sqlFilePaths)
        {
            var sqlScript = File.ReadAllText(sqlFilePath);
            sb.Append(sqlScript + Environment.NewLine);
        }

        if (!string.IsNullOrWhiteSpace(dbName))
        {
            sb.Replace("DefaultDbName", dbName);
        }

        return sb.ToString();
    }
}
