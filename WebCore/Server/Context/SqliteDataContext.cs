using GlobalInfraction.WebCore.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Context;

public class SqliteDataContext : DbContext
{
    public SqliteDataContext(DbContextOptions<SqliteDataContext> options) : base(options)
    {
    }

    public DbSet<EFInstance> Instances { get; set; }
    public DbSet<EFProfile> Profiles { get; set; }
    public DbSet<EFAlias> ProfileMetas { get; set; }
    public DbSet<EFInfraction> Infractions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EFInstance>().ToTable("EFInstances");
        modelBuilder.Entity<EFProfile>().ToTable("EFProfiles");
        modelBuilder.Entity<EFAlias>().ToTable("EFAliases");
        modelBuilder.Entity<EFInfraction>().ToTable("EFInfractions");

        modelBuilder.Entity<EFInfraction>()
            .HasOne(i => i.Admin)
            .WithOne()
            .HasForeignKey<EFInfraction>(i => i.AdminId);

        var adminProfile = new EFProfile
        {
            Id = -1,
            ProfileIdentity = "MDpVS04=",
            Reputation = 0,
            Infractions = new List<EFInfraction>()
        };

        var adminAlias = new EFAlias
        {
            Id = -1,
            ProfileId = -1,
            UserName = "IW4MAdmin",
            IpAddress = "0.0.0.0",
            Changed = DateTimeOffset.UtcNow,
        };


        modelBuilder.Entity<EFProfile>().HasData(adminProfile);
        modelBuilder.Entity<EFAlias>().HasData(adminAlias);

        base.OnModelCreating(modelBuilder);
    }
}
