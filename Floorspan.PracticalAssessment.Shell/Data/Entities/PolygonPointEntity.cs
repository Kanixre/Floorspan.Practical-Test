namespace Floorspan.PracticalAssessment.Shell.Data.Entities;

public sealed class PolygonPointEntity
{
    public long Id { get; set; }
    public Guid PolygonId { get; set; }
    public int Sequence { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public PolygonEntity Polygon { get; set; } = null!;
}
