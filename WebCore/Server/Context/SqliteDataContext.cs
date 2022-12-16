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
    public DbSet<EFProfileMeta> ProfileMetas { get; set; }
    public DbSet<EFInfraction> Infractions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EFInstance>().ToTable("EFInstances");
        modelBuilder.Entity<EFProfile>().ToTable("EFProfiles");
        modelBuilder.Entity<EFProfileMeta>().ToTable("EFProfileMetas");
        modelBuilder.Entity<EFInfraction>().ToTable("EFInfractions");
        base.OnModelCreating(modelBuilder);
    }
}
