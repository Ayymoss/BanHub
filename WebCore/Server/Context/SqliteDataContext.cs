using GlobalInfraction.WebCore.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Context;

public class SqliteDataContext : DbContext
{
    public SqliteDataContext(DbContextOptions<SqliteDataContext> options) : base(options)
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
            .HasOne(i => i.Admin)
            .WithOne()
            .HasForeignKey<EFInfraction>(i => i.AdminId);

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
            ProfileIdentity = "0:UKN",
            HeartBeat = DateTimeOffset.UtcNow,
            Reputation = 0,
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

        base.OnModelCreating(modelBuilder);
    }
}
