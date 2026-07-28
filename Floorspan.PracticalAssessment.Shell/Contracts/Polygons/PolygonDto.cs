using Floorspan.PracticalAssessment.Shell.Contracts.Geometry;

namespace Floorspan.PracticalAssessment.Shell.Contracts.Polygons;

public sealed class PolygonDto
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedUtc { get; init; }
    public IReadOnlyList<Point2Dto> Points { get; init; } = Array.Empty<Point2Dto>();
    public Point2Dto? Origin { get; init; }
    public Vector2Dto? U1 { get; init; }
    public Vector2Dto? U2 { get; init; }
}
