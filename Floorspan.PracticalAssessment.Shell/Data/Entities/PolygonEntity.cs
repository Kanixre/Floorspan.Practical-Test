namespace Floorspan.PracticalAssessment.Shell.Data.Entities;

public sealed class PolygonEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }

    public double? OriginX { get; set; }
    public double? OriginY { get; set; }

    public double? U1X { get; set; }
    public double? U1Y { get; set; }

    public double? U2X { get; set; }
    public double? U2Y { get; set; }

    public ICollection<PolygonPointEntity> Points { get; set; } = new List<PolygonPointEntity>();
}
