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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EFInstance>().ToTable("EFInstances");
        modelBuilder.Entity<EFEntity>().ToTable("EFEntities");
        modelBuilder.Entity<EFAlias>().ToTable("EFAliases");
        modelBuilder.Entity<EFInfraction>().ToTable("EFInfractions");
        modelBuilder.Entity<EFCurrentAlias>().ToTable("EFCurrentAliases");

        modelBuilder.Entity<EFInfraction>()
            .HasOne(a => a.Target)
            .WithMany(p => p.Infractions)
            .HasForeignKey(f => f.TargetId);

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
            Reputation = 0,
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

        // TODO: Remove once tested.

        #region TEMPORARY_SEED_DATA

        var adminAliasTwo = new EFAlias
        {
            Id = -2,
            EntityId = -1,
            UserName = "AdminMan",
            IpAddress = "0.0.0.0",
            Changed = DateTimeOffset.UtcNow + TimeSpan.FromHours(2)
        };

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

        modelBuilder.Entity<EFAlias>().HasData(adminAliasTwo);
        modelBuilder.Entity<EFInstance>().HasData(instance);
        modelBuilder.Entity<EFInfraction>().HasData(infraction);

        #endregion

        base.OnModelCreating(modelBuilder);
    }
}
