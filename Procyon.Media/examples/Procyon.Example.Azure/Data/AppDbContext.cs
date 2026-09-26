using Microsoft.EntityFrameworkCore;

namespace Procyon.Example.Azure.Data;

public class AppDbContext : DbContext
{
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
