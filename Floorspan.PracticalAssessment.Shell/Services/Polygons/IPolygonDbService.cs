using Floorspan.PracticalAssessment.Shell.Data.Entities;

namespace Floorspan.PracticalAssessment.Shell.Services.Polygons;

/// <summary>
/// Defines the persistence operations required by the polygon application service.
/// </summary>
public interface IPolygonDbService
{
    Task<IReadOnlyList<PolygonEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<PolygonEntity> CreateAsync(
        PolygonEntity entity,
        CancellationToken cancellationToken = default);
}