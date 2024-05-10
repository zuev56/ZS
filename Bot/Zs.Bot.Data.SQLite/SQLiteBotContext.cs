using Microsoft.EntityFrameworkCore;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Data.SQLite;

public class SQLiteBotContext : BotContext<SQLiteBotContext>
{
    private const string UtcNow = "datetime('now')";

    public SQLiteBotContext()
    {
    }

    public SQLiteBotContext(DbContextOptions<SQLiteBotContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEntities(modelBuilder);
    }

    private static void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(b =>
        {
            b.HasKey(e => e.Id);

            b.Property(e => e.Type);
            b.Property(e => e.Name).HasMaxLength(100);
            b.Property(e => e.RawData);
            b.Property(e => e.CreatedAt).HasDefaultValueSql(UtcNow).ValueGeneratedOnAdd();
            b.Property(e => e.UpdatedAt).HasDefaultValueSql(UtcNow).ValueGeneratedOnUpdate();
        });

        modelBuilder.Entity<Message>(b =>
        {
            b.HasKey(e => e.Id);

            b.Property(e => e.ChatId);
            b.Property(e => e.UserId);
            b.Property(e => e.ReplyToMessageId);
            b.Property(e => e.IsDeleted);
            b.Property(e => e.RawData);
            b.Property(e => e.CreatedAt).HasDefaultValueSql(UtcNow).ValueGeneratedOnAdd();
            b.Property(e => e.UpdatedAt).HasDefaultValueSql(UtcNow).ValueGeneratedOnUpdate();

            b.HasOne(m => m.User).WithMany().HasForeignKey(e => e.UserId);
            b.HasOne(m => m.Chat).WithMany().HasForeignKey(e => e.ChatId);

            b.HasIndex(e => e.ReplyToMessageId);
            b.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(e => e.Id);

            b.Property(e => e.Role);
            b.Property(e => e.UserName).HasMaxLength(100);
            b.Property(e => e.FullName).HasMaxLength(100);
            b.Property(e => e.RawData);
            b.Property(e => e.CreatedAt).HasDefaultValueSql(UtcNow).ValueGeneratedOnAdd();
            b.Property(e => e.UpdatedAt).HasDefaultValueSql(UtcNow).ValueGeneratedOnUpdate();

            b.HasIndex(e => e.Role);
        });
    }
}