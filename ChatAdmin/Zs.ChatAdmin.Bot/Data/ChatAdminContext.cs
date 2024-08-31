using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using ChatAdmin.Bot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Zs.Bot.Data.Models;
using Zs.Bot.Data.PostgreSQL;
using Zs.Common.Extensions;

namespace ChatAdmin.Bot.Data;

internal sealed class ChatAdminContext : DbContext
{
    public DbSet<Accounting> Accountings { get; set; }
    public DbSet<AuxiliaryWord> AuxiliaryWords { get; set; }
    public DbSet<Ban> Bans { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public ChatAdminContext()
    {
    }

    public ChatAdminContext(DbContextOptions<ChatAdminContext> options)
       : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        PostgreSqlBotContext.ConfigureEntities(modelBuilder);
        PostgreSqlBotContext.SeedData(modelBuilder);

        ConfigureEntities(modelBuilder);
        SeedData(modelBuilder, dbChatId: 5); // ¯\_(ツ)_/¯
    }

    private static void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accounting>(b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("id")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            b.Property<DateTime>("StartDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("start_date")
                .HasDefaultValueSql("now()");

            b.Property<DateTime>("UpdateDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("update_date")
                .HasDefaultValueSql("now()");

            b.HasKey("Id");

            b.ToTable("accountings", "ca");
        });

        modelBuilder.Entity<AuxiliaryWord>(b =>
        {
            b.Property<string>("Word")
                .HasMaxLength(100)
                .HasColumnType("character varying(100)")
                .HasColumnName("id");

            b.Property<DateTime>("InsertDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("insert_date")
                .HasDefaultValueSql("now()");

            b.HasKey("Word");

            b.ToTable("auxiliary_words", "ca");
        });

        modelBuilder.Entity<Ban>(b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("id")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            b.Property<int>("ChatId")
                .HasColumnType("integer")
                .HasColumnName("chat_id");

            b.Property<int>("UserId")
                .HasColumnType("integer")
                .HasColumnName("user_id");

            b.Property<int?>("WarningMessageId")
                .HasColumnType("integer")
                .HasColumnName("warning_message_id");

            b.Property<DateTime?>("FinishDate")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("finish_date");

            b.Property<DateTime>("InsertDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("insert_date")
                .HasDefaultValueSql("now()");

            b.Property<DateTime>("UpdateDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("update_date")
                .HasDefaultValueSql("now()");

            b.HasKey("Id");

            b.HasIndex("ChatId");

            b.HasIndex("UserId");

            b.HasIndex("WarningMessageId");

            b.ToTable("bans", "ca");
        });

        modelBuilder.Entity<Notification>(b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("id")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            b.Property<int?>("Month")
                .HasColumnType("integer")
                .HasColumnName("month");

            b.Property<int>("Day")
                .HasColumnType("integer")
                .HasColumnName("day");

            b.Property<int>("Hour")
                .HasColumnType("integer")
                .HasColumnName("hour");

            b.Property<int>("Minute")
                .HasColumnType("integer")
                .HasColumnName("minute");

            b.Property<string>("Message")
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnType("character varying(2000)")
                .HasColumnName("message");

            b.Property<DateTime?>("ExecDate")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("exec_date");

            b.Property<bool>("IsActive")
                .HasColumnType("boolean")
                .HasColumnName("is_active");

            b.Property<DateTime>("InsertDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("insert_date")
                .HasDefaultValueSql("now()");

            b.Property<DateTime>("UpdateDate")
                .ValueGeneratedOnAdd()
                .HasColumnType("timestamp with time zone")
                .HasColumnName("update_date")
                .HasDefaultValueSql("now()");

            b.HasKey("Id");

            b.ToTable("notifications", "ca");
        });
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
    private static void SeedData(ModelBuilder modelBuilder, int dbChatId)
    {
        modelBuilder.Entity<Command>().HasData(new[]
        {
            new Command()
            {
                Id = "/GetUserStatistics".ToLowerInvariant(),
                Script = $@"SELECT string_agg(function_results.val, ' ') as Result
                               FROM (
                                   SELECT * FROM (
                                       SELECT ca.sf_cmd_get_full_statistics({{0}}, {{1}}, {{2}}) as val, 1 as order
                                       UNION
                                       SELECT chr(10) || chr(10) || 'The most popular words:' || chr(10) || string_agg(w.word || ' (' || w.count || ')', ',  ') as val, 2 as order
                                       FROM (SELECT * FROM ca.sf_get_most_popular_words({dbChatId}, {{1}}, {{2}}, 2) LIMIT 10) w
                                       UNION
                                       SELECT chr(10) || chr(10) || 'Bans and warnings:' || chr(10) || string_agg(user_name || ' - ' || status, chr(10)) as val, 3 as order
                                       FROM (SELECT * FROM ca.sf_get_bans({dbChatId}, {{1}}, {{2}})) b
                                   ) r
                               ORDER BY r.order
                               ) function_results",
                DefaultArgs = "15; now()::Date; now()",
                Description = "Получение статистики по активности участников всех чатов за определённый период",
                Group = "adminCmdGroup"},
        });
    }

    public static string GetOtherSqlScripts(string configPath)
    {
        var configuration = new ConfigurationBuilder()
              .AddJsonFile(System.IO.Path.GetFullPath(configPath))
              .Build();

        var connectionStringBuilder = new DbConnectionStringBuilder()
        {
            ConnectionString = configuration.GetSecretValue("ConnectionStrings:Default")
        };
        var dbName = connectionStringBuilder["Database"] as string;

        var dataDirPath = Directory.GetCurrentDirectory() == "/app" ? "./Data" : "../../../Data";

        var sqlFilePaths = Directory.GetFiles(dataDirPath, "*.sql", SearchOption.AllDirectories).ToList();

        var sb = new StringBuilder();
        foreach (var sqlFilePath in sqlFilePaths)
        {
            var sqlScript = File.ReadAllText(sqlFilePath);
            sb.Append(sqlScript + Environment.NewLine);
        }

        if (!string.IsNullOrWhiteSpace(dbName))
            sb.Replace("DefaultDbName", dbName);

        return sb.ToString();
    }
}
