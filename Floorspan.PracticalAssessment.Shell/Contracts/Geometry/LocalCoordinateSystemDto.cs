namespace Floorspan.PracticalAssessment.Shell.Contracts.Geometry;

public sealed record LocalCoordinateSystem(
    Point2Dto Origin,
    Point2Dto U1,
    Point2Dto U2
);
