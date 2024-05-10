using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zs.Bot.Data.Models;

namespace Zs.Bot.Data;

public abstract class BotContext<TContext> : DbContext
    where TContext : DbContext
{
    public DbSet<Chat> Chats { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    protected BotContext()
    {
    }

    protected BotContext(DbContextOptions<TContext> options)
        : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken).ConfigureAwait(false);
    }

    private void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries();
        var utcNow = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.Entity is DbEntity trackableEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        trackableEntity.UpdatedAt = utcNow;
                        entry.Property(nameof(DbEntity.CreatedAt)).IsModified = false;
                        break;

                    case EntityState.Added:
                        trackableEntity.CreatedAt = utcNow;
                        trackableEntity.UpdatedAt = utcNow;
                        break;
                }
            }
        }
    }
}