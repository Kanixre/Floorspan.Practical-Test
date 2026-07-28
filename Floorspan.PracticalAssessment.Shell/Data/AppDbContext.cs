using Floorspan.PracticalAssessment.Shell.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Floorspan.PracticalAssessment.Shell.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PolygonEntity> Polygons => Set<PolygonEntity>();
    public DbSet<PolygonPointEntity> PolygonPoints => Set<PolygonPointEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PolygonEntity>(entity =>
        {
            entity.ToTable("Polygons");
            entity.HasKey(x => x.Id);

            entity.HasMany(x => x.Points)
                .WithOne(x => x.Polygon)
                .HasForeignKey(x => x.PolygonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PolygonPointEntity>(entity =>
        {
            entity.ToTable("PolygonPoints");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PolygonId, x.Sequence }).IsUnique();
        });
    }
}
