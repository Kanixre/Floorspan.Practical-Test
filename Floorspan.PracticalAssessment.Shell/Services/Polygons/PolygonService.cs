using Floorspan.PracticalAssessment.Shell.Contracts.Geometry;
using Floorspan.PracticalAssessment.Shell.Contracts.Polygons;
using Floorspan.PracticalAssessment.Shell.Data.Entities;
using Floorspan.PracticalAssessment.Shell.Services.Geometry;

namespace Floorspan.PracticalAssessment.Shell.Services.Polygons;

/// <summary>
/// Coordinates polygon validation, geometry calculation, persistence and mapping.
///
/// This service represents the application layer. It should coordinate the
/// database and geometry services without containing HTTP-specific behaviour.
/// </summary>
public sealed class PolygonService(
    IPolygonDbService polygonDbService,
    IOrientedBoundingBoxService orientedBoundingBoxService) : IPolygonService
{

    public async Task<IReadOnlyList<PolygonDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var polygonEntities = await polygonDbService.GetAllAsync(cancellationToken);

        var polygonDtos = polygonEntities
            .Select(MapToDto)
            .OrderBy(dto => dto.Id)
            .ToList();

        return polygonDtos;
    }


    public async Task<PolygonDto> CreateAsync(CreatePolygonRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate the incoming request
        ValidateRequest(request);

        // 2. Extract the ordered collection of points
        var points = request.Points;

        // 3. Reject inputs that cannot form a meaningful polygon
        ValidatePoints(points);

        // 4. Pass the points to IOrientedBoundingBoxService
        var localCoordinateSystem = orientedBoundingBoxService.Calculate(points);

        // 5. Create the persistence model required by IPolygonDbService
        //    6. Preserve the input order of the polygon points
        var polygonEntity = new PolygonEntity
        {
            OriginX = localCoordinateSystem.Origin.X,
            OriginY = localCoordinateSystem.Origin.Y,
            U1X = localCoordinateSystem.U1.X,
            U1Y = localCoordinateSystem.U1.Y,
            U2X = localCoordinateSystem.U2.X,
            U2Y = localCoordinateSystem.U2.Y,
            Points = points
                .Select((p, index) => new PolygonPointEntity
                {
                    X = p.X,
                    Y = p.Y,
                    Sequence = index
                })
                .ToList()
        };

        // 7. Store the polygon asynchronously
        var storedEntity = await polygonDbService.CreateAsync(polygonEntity, cancellationToken);

        // 8. Convert the stored result into PolygonDto
        return MapToDto(storedEntity);
    }

    private static void ValidateRequest(CreatePolygonRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Points is null)
        {
            throw new PolygonValidationException("Request must contain a collection of points.");
        }
    }

    /// <summary>
    /// Validates whether the supplied points can be processed as a polygon.
    /// </summary>
    private static void ValidatePoints(IReadOnlyCollection<Point2Dto> points)
    {
        if (points is null)
        {
            throw new PolygonValidationException("Points collection must not be null.");
        }

        if (points.Count < 3)
        {
            throw new PolygonValidationException(
                $"At least three points are required, but {points.Count} were supplied.");
        }

        foreach (var p in points)
        {
            if (!double.IsFinite(p.X) || !double.IsFinite(p.Y))
            {
                throw new PolygonValidationException(
                    $"Coordinates must be finite values. Found ({p.X}, {p.Y}).");
            }
        }

        var distinctCount = points
            .Select(p => (p.X, p.Y))
            .Distinct()
            .Count();

        if (distinctCount < 3)
        {
            throw new PolygonValidationException(
                "At least three distinct points are required.");
        }

        var pointList = points as IReadOnlyList<Point2Dto> ?? points.ToList();

        for (int i = 0; i < pointList.Count; i++)
        {
            var current = pointList[i];
            var next = pointList[(i + 1) % pointList.Count];

            if (IsApproximatelyEqual(current, next))
            {
                throw new PolygonValidationException(
                    $"Consecutive duplicate point found at index {i}.");
            }
        }
    }

    private static bool IsApproximatelyEqual(Point2Dto a, Point2Dto b, double tolerance = 1e-9)
    {
        return Math.Abs(a.X - b.X) < tolerance && Math.Abs(a.Y - b.Y) < tolerance;
    }

    //private static PolygonDto MapToDto(PolygonEntity entity)
    //{
    //    var orderedPoints = entity.Points
    //        .OrderBy(p => p.Sequence)
    //        .Select(p => new Point2Dto(p.X, p.Y))
    //        .ToList();

    //    return new PolygonDto(entity.Id, orderedPoints);
    //}

    public sealed class PolygonValidationException : Exception
    {
        public PolygonValidationException(string message) : base(message) { }
    }

    /// <summary>
    /// Converts a persisted polygon into its API contract representation.
    /// </summary>
    /// <remarks>
    /// The resulting PolygonDto should include:
    ///
    /// - The polygon identifier.
    /// - Its creation timestamp.
    /// - Its ordered points.
    /// - The calculated origin.
    /// - The primary direction vector.
    /// - The secondary direction vector.
    ///
    /// The point sequence stored by the database must be respected.
    ///
    /// Consider how nullable persisted geometry values should be represented.
    /// </remarks>
    
    private static PolygonDto MapToDto(PolygonEntity entity)
    {
        var orderedPoints = entity.Points
            .OrderBy(p => p.Sequence)
            .Select(p => new Point2Dto(p.X, p.Y))
            .ToList();

        Point2Dto? origin = (entity.OriginX.HasValue && entity.OriginY.HasValue)
            ? new Point2Dto(entity.OriginX.Value, entity.OriginY.Value)
            : null;

        Vector2Dto? u1 = (entity.U1X.HasValue && entity.U1Y.HasValue)
            ? new Vector2Dto(entity.U1X.Value, entity.U1Y.Value)
            : null;

        Vector2Dto? u2 = (entity.U2X.HasValue && entity.U2Y.HasValue)
            ? new Vector2Dto(entity.U2X.Value, entity.U2Y.Value)
            : null;

        return new PolygonDto
        {
            Id = entity.Id,
            CreatedUtc = entity.CreatedUtc,
            Points = orderedPoints,
            Origin = origin,
            U1 = u1,
            U2 = u2
        };
    }


}