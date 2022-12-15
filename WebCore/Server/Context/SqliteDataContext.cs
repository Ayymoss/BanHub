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
}
