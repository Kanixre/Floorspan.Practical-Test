using Floorspan.PracticalAssessment.Shell.Contracts.Polygons;

namespace Floorspan.PracticalAssessment.Shell.Services.Api;

public interface IPolygonApi
{
    Task<IReadOnlyList<PolygonDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PolygonDto> CreateAsync(
        CreatePolygonRequest request,
        CancellationToken cancellationToken = default);
}
