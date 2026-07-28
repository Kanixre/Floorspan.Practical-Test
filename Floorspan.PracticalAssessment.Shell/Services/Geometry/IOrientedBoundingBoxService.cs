using Floorspan.PracticalAssessment.Shell.Contracts.Geometry;

namespace Floorspan.PracticalAssessment.Shell.Services.Geometry;

public interface IOrientedBoundingBoxService
{
    OrientedBoundingBoxResult Calculate(IReadOnlyCollection<Point2Dto> points);
}

public sealed record OrientedBoundingBoxResult(
    Point2Dto Origin,
    Vector2Dto U1,
    Vector2Dto U2);
