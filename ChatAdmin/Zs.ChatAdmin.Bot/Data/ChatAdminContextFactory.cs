using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Zs.Common.Extensions;

namespace ChatAdmin.Bot.Data;

internal sealed class ChatAdminContextFactory : IDbContextFactory<ChatAdminContext>, IDesignTimeDbContextFactory<ChatAdminContext>
{
    private readonly DbContextOptions<ChatAdminContext> _options;

    public ChatAdminContextFactory()
    {
    }

    public ChatAdminContextFactory(DbContextOptions<ChatAdminContext> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    // For repositories
    public ChatAdminContext CreateDbContext() => new ChatAdminContext(_options);

    // For migrations
    public ChatAdminContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetSecretValue("ConnectionStrings:Default");
        var optionsBuilder = new DbContextOptionsBuilder<ChatAdminContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ChatAdminContext(optionsBuilder.Options);
    }
}
