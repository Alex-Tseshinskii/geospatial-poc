using Microsoft.EntityFrameworkCore;

namespace Calculation.PostgreSql.Database;

public class GeospatialDbContext(DbContextOptions<GeospatialDbContext> options) : DbContext(options)
{
    public DbSet<GeofenceEntity> Geofences { get; set; }
}