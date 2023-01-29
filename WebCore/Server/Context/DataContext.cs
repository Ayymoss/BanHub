using BanHub.WebCore.Server.Models;
using BanHub.WebCore.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Context;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<EFInstance> Instances { get; set; }
    public DbSet<EFEntity> Entities { get; set; }
    public DbSet<EFAlias> Aliases { get; set; }
    public DbSet<EFCurrentAlias> CurrentAliases { get; set; }
    public DbSet<EFInfraction> Infractions { get; set; }
    public DbSet<EFServer> Servers { get; set; }
    public DbSet<EFServerConnection> ServerConnections { get; set; }
    public DbSet<EFStatistic> Statistics { get; set; }
    public DbSet<EFAuthToken> AuthTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EFInstance>().ToTable("EFInstances");
        modelBuilder.Entity<EFEntity>().ToTable("EFEntities");
        modelBuilder.Entity<EFAlias>().ToTable("EFAliases");
        modelBuilder.Entity<EFInfraction>().ToTable("EFInfractions");
        modelBuilder.Entity<EFCurrentAlias>().ToTable("EFCurrentAliases");
        modelBuilder.Entity<EFServer>().ToTable("EFServers");
        modelBuilder.Entity<EFServerConnection>().ToTable("EFServerConnections");
        modelBuilder.Entity<EFStatistic>().ToTable("EFStatistics");
        modelBuilder.Entity<EFAuthToken>().ToTable("EFAuthTokens");
        modelBuilder.Entity<EFNote>().ToTable("EFNotes");

        modelBuilder.Entity<EFInfraction>()
            .HasOne(a => a.Target)
            .WithMany(p => p.Infractions)
            .HasForeignKey(f => f.TargetId);
        
        modelBuilder.Entity<EFNote>()
            .HasOne(a => a.Target)
            .WithMany(p => p.Notes)
            .HasForeignKey(f => f.TargetId);

        #region EFStatistic

        var instanceCount = new EFStatistic
        {
            Id = -1,
            Statistic = "InstanceCount",
            Count = 0
        };

        var infractionCount = new EFStatistic
        {
            Id = -2,
            Statistic = "InfractionCount",
            Count = 0
        };

        var serverCount = new EFStatistic
        {
            Id = -3,
            Statistic = "ServerCount",
            Count = 0
        };

        var entityCount = new EFStatistic
        {
            Id = -4,
            Statistic = "EntityCount",
            Count = 1
        };
        
        modelBuilder.Entity<EFStatistic>().HasData(instanceCount);
        modelBuilder.Entity<EFStatistic>().HasData(infractionCount);
        modelBuilder.Entity<EFStatistic>().HasData(serverCount);
        modelBuilder.Entity<EFStatistic>().HasData(entityCount);

        #endregion

        #region IW4MAdminSeed

        var adminAlias = new EFAlias
        {
            Id = -1,
            EntityId = -1,
            UserName = "IW4MAdmin",
            IpAddress = "0.0.0.0",
            Changed = DateTimeOffset.UtcNow
        };

        var adminProfile = new EFEntity
        {
            Id = -1,
            Identity = "0:UKN",
            HeartBeat = DateTimeOffset.UtcNow,
            Created = DateTimeOffset.UtcNow,
            WebRole = WebRole.User,
            Infractions = new List<EFInfraction>()
        };

        var adminCurrentAlias = new EFCurrentAlias
        {
            Id = -1,
            EntityId = -1,
            AliasId = -1
        };

        modelBuilder.Entity<EFEntity>().HasData(adminProfile);
        modelBuilder.Entity<EFAlias>().HasData(adminAlias);
        modelBuilder.Entity<EFCurrentAlias>().HasData(adminCurrentAlias);

        #endregion
        
        base.OnModelCreating(modelBuilder);
    }
}
