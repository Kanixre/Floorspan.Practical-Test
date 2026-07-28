using Floorspan.PracticalAssessment.Shell.Contracts.Polygons;

namespace Floorspan.PracticalAssessment.Shell.Services.Polygons;

public interface IPolygonService
{
    Task<IReadOnlyList<PolygonDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PolygonDto> CreateAsync(
        CreatePolygonRequest request,
        CancellationToken cancellationToken = default);
}
