using BanHub.WebCore.Server.Models.Domains;
using BanHubData.Enums;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Context;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<EFInstance> Instances { get; set; }
    public DbSet<EFPlayer> Players { get; set; }
    public DbSet<EFAlias> Aliases { get; set; }
    public DbSet<EFCurrentAlias> CurrentAliases { get; set; }
    public DbSet<EFPenalty> Penalties { get; set; }
    public DbSet<EFServer> Servers { get; set; }
    public DbSet<EFServerConnection> ServerConnections { get; set; }
    public DbSet<EFAuthToken> AuthTokens { get; set; }
    public DbSet<EFNote> Notes { get; set; }
    public DbSet<EFPenaltyIdentifier> PenaltyIdentifiers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EFInstance>().ToTable("EFInstances");
        modelBuilder.Entity<EFPlayer>().ToTable("EFPlayers");
        modelBuilder.Entity<EFAlias>().ToTable("EFAliases");
        modelBuilder.Entity<EFPenalty>().ToTable("EFPenalties");
        modelBuilder.Entity<EFCurrentAlias>().ToTable("EFCurrentAliases");
        modelBuilder.Entity<EFServer>().ToTable("EFServers");
        modelBuilder.Entity<EFServerConnection>().ToTable("EFServerConnections");
        modelBuilder.Entity<EFAuthToken>().ToTable("EFAuthTokens");
        modelBuilder.Entity<EFNote>().ToTable("EFNotes");
        modelBuilder.Entity<EFPenaltyIdentifier>().ToTable("EFPenaltyIdentifiers");

        modelBuilder.Entity<EFPenalty>()
            .HasOne(a => a.Target)
            .WithMany(p => p.Penalties)
            .HasForeignKey(f => f.TargetId);

        modelBuilder.Entity<EFNote>()
            .HasOne(a => a.Target)
            .WithMany(p => p.Notes)
            .HasForeignKey(f => f.TargetId);

        #region IW4MAdminSeed

        var adminAlias = new EFAlias
        {
            Id = -1,
            PlayerId = -1,
            UserName = "IW4MAdmin",
            IpAddress = "0.0.0.0",
            Changed = DateTimeOffset.UtcNow
        };

        var adminProfile = new EFPlayer
        {
            Id = -1,
            Identity = "0:UKN",
            HeartBeat = DateTimeOffset.UtcNow,
            Created = DateTimeOffset.UtcNow,
            WebRole = WebRole.WebUser,
            InstanceRole = InstanceRole.InstanceUser,
            Penalties = new List<EFPenalty>(),
            TotalConnections = 0,
            PlayTime = TimeSpan.Zero
        };

        var adminCurrentAlias = new EFCurrentAlias
        {
            Id = -1,
            PlayerId = -1,
            AliasId = -1
        };

        modelBuilder.Entity<EFPlayer>().HasData(adminProfile);
        modelBuilder.Entity<EFAlias>().HasData(adminAlias);
        modelBuilder.Entity<EFCurrentAlias>().HasData(adminCurrentAlias);

        #endregion


        #region TemporarySeedData

        var instance = new EFInstance
        {
            Id = -1,
            InstanceGuid = Guid.NewGuid(),
            InstanceIp = "123.123.123.123",
            InstanceName = "Seed Instance",
            HeartBeat = DateTimeOffset.UtcNow,
            ApiKey = Guid.NewGuid(),
            Active = true,
            Created = DateTimeOffset.UtcNow,
            About = "Some description about the instance.",
            Socials = new Dictionary<string, string>
            {
                {"YouTube", "https://www.youtube.com/watch?v=dQw4w9WgXcQ"},
                {"Another YouTube", "https://www.youtube.com/watch?v=sFce1pBvSd4"}
            },
        };

        var infraction = new EFPenalty
        {
            Id = -1,
            PenaltyType = PenaltyType.Ban,
            PenaltyStatus = PenaltyStatus.Active,
            PenaltyScope = PenaltyScope.Global,
            PenaltyGuid = Guid.NewGuid(),
            Submitted = DateTimeOffset.UtcNow,
            Duration = null,
            Reason = "Seed Infraction",
            Evidence = "https://www.youtube.com/watch?v=WePNs-G7puA",
            AdminId = -1,
            TargetId = -1,
            InstanceId = -1,
            Automated = true
        };

        modelBuilder.Entity<EFInstance>().HasData(instance);
        modelBuilder.Entity<EFPenalty>().HasData(infraction);

        #endregion

        base.OnModelCreating(modelBuilder);
    }
}
