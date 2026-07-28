using Floorspan.PracticalAssessment.Shell.Contracts.Geometry;

namespace Floorspan.PracticalAssessment.Shell.Contracts.Polygons;

public sealed class CreatePolygonRequest
{
    public IReadOnlyCollection<Point2Dto> Points { get; init; } = Array.Empty<Point2Dto>();
}
