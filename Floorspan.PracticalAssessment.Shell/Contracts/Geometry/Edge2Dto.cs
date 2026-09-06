namespace Floorspan.PracticalAssessment.Shell.Contracts.Geometry;

// public sealed record Edge2Dto(double start, double end, double Length);

public record Edge2Dto(Vector2Dto Start, Vector2Dto End, double Length);
