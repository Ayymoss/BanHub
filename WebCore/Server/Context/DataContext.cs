using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Context;

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

        modelBuilder.Entity<EFInfraction>()
            .HasOne(a => a.Target)
            .WithMany(p => p.Infractions)
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
            Count = 0
        };

        var aliasCount = new EFStatistic
        {
            Id = -5,
            Statistic = "AliasCount",
            Count = 0
        };

        modelBuilder.Entity<EFStatistic>().HasData(instanceCount);
        modelBuilder.Entity<EFStatistic>().HasData(infractionCount);
        modelBuilder.Entity<EFStatistic>().HasData(serverCount);
        modelBuilder.Entity<EFStatistic>().HasData(entityCount);
        modelBuilder.Entity<EFStatistic>().HasData(aliasCount);

        #endregion

        #region SystemSeed

        var systemAlias = new EFAlias
        {
            Id = -1,
            EntityId = -1,
            UserName = "> System",
            IpAddress = "0.0.0.0",
            Changed = DateTimeOffset.UtcNow
        };

        var systemProfile = new EFEntity
        {
            Id = -1,
            Identity = "000:SYS",
            HeartBeat = DateTimeOffset.UtcNow,
            Reputation = 0,
            Created = DateTimeOffset.UtcNow,
            WebRole = WebRole.User,
            Infractions = new List<EFInfraction>()
        };

        var systemCurrentAlias = new EFCurrentAlias
        {
            Id = -1,
            EntityId = -1,
            AliasId = -1
        };

        modelBuilder.Entity<EFEntity>().HasData(systemProfile);
        modelBuilder.Entity<EFAlias>().HasData(systemAlias);
        modelBuilder.Entity<EFCurrentAlias>().HasData(systemCurrentAlias);

        #endregion

        #region IW4MAdminSeed

        var adminAlias = new EFAlias
        {
            Id = -2,
            EntityId = -2,
            UserName = "IW4MAdmin",
            IpAddress = "0.0.0.0",
            Changed = DateTimeOffset.UtcNow
        };

        var adminProfile = new EFEntity
        {
            Id = -2,
            Identity = "0:UKN",
            HeartBeat = DateTimeOffset.UtcNow,
            Reputation = 0,
            Created = DateTimeOffset.UtcNow,
            WebRole = WebRole.User,
            Infractions = new List<EFInfraction>()
        };

        var adminCurrentAlias = new EFCurrentAlias
        {
            Id = -2,
            EntityId = -2,
            AliasId = -2
        };

        modelBuilder.Entity<EFEntity>().HasData(adminProfile);
        modelBuilder.Entity<EFAlias>().HasData(adminAlias);
        modelBuilder.Entity<EFCurrentAlias>().HasData(adminCurrentAlias);

        #endregion

        // TODO: Remove once tested.

        #region TemporarySeed
        var instance = new EFInstance
        {
            Id = -1,
            InstanceGuid = Guid.NewGuid(),
            InstanceIp = "123.123.123.123",
            InstanceName = "Seed Instance",
            HeartBeat = DateTimeOffset.UtcNow,
            ApiKey = Guid.NewGuid(),
            Active = true
        };

        var infraction = new EFInfraction
        {
            Id = -1,
            InfractionType = InfractionType.Warn,
            InfractionStatus = InfractionStatus.Active,
            InfractionScope = InfractionScope.Local,
            InfractionGuid = Guid.NewGuid(),
            Submitted = DateTimeOffset.UtcNow,
            Duration = null,
            Reason = "Seed Infraction",
            Evidence = "Seed Evidence",
            AdminId = -1,
            TargetId = -1,
            InstanceId = -1
        };

        modelBuilder.Entity<EFInstance>().HasData(instance);
        modelBuilder.Entity<EFInfraction>().HasData(infraction);
        #endregion

        base.OnModelCreating(modelBuilder);
    }
}
