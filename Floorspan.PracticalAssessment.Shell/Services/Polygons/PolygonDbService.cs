using Floorspan.PracticalAssessment.Shell.Data;
using Floorspan.PracticalAssessment.Shell.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Floorspan.PracticalAssessment.Shell.Services.Polygons;

/// <summary>
/// Provides persistence operations for polygon entities.
///
/// This service should contain EF Core-specific behaviour so that the
/// application service does not need to interact with AppDbContext directly.
/// </summary>
public sealed class PolygonDbService(AppDbContext db) : IPolygonDbService
{
    /// <summary>
    /// Retrieves all stored polygons and their associated points.
    /// </summary>
    /// <remarks>
    /// Requirements:
    ///
    /// - Query the polygon collection asynchronously.
    /// - Include each polygon's associated points.
    /// - Avoid unnecessary EF Core change tracking for this read operation.
    /// - Preserve enough information for the application service to restore
    ///   the original ordering of the points.
    /// - Pass the cancellation token to the EF Core operation.
    ///
    /// Consider whether sorting should happen in the database query or in the
    /// application service. Be prepared to justify the decision.
    /// </remarks>

    public async Task<IReadOnlyList<PolygonEntity>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await db.Polygons
            .AsNoTracking()
            .Include(p => p.Points)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds and saves a new polygon.
    /// </summary>
    /// <remarks>
    /// Requirements:
    ///
    /// - Add the supplied entity to AppDbContext.
    /// - Persist both the polygon and its child point entities.
    /// - Use asynchronous EF Core APIs.
    /// - Pass the cancellation token to the save operation.
    /// - Return the stored entity.
    ///
    /// The caller is responsible for constructing the entity and calculating
    /// its geometry. This service is responsible only for persistence.
    ///
    /// Consider:
    ///
    /// - How EF Core discovers and inserts the child point entities.
    /// - Whether SaveChangesAsync can return without the entity identifiers
    ///   being populated.
    /// - What should happen if the operation is cancelled.
    /// </remarks>
    public async Task<PolygonEntity> CreateAsync(
    PolygonEntity entity,
    CancellationToken cancellationToken = default)
    {
        db.Polygons.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }
}