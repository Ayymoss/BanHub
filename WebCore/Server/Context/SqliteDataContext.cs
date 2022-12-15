using GlobalBan.WebCore.Server.Models;
using GlobalBan.WebCore.Shared;
using Microsoft.EntityFrameworkCore;

namespace GlobalBan.WebCore.Server.Context;

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
