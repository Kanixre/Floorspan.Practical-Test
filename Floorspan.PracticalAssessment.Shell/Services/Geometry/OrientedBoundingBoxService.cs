using Floorspan.PracticalAssessment.Shell.Contracts.Geometry;

namespace Floorspan.PracticalAssessment.Shell.Services.Geometry;

/// <summary>
/// Calculates an approximate local coordinate system for a polygon.
///
/// The result should describe:
///
/// - An origin associated with the polygon's oriented bounds.
/// - A primary unit vector representing the polygon's dominant direction.
/// - A secondary unit vector perpendicular to the primary vector.
///
/// There are several valid approaches to this problem. The implementation
/// should be deterministic, mathematically defensible and clearly explained.
/// </summary>
/// 
/// <summary>
/// Calculates an approximate oriented bounding-box basis for the supplied points.
/// </summary>
/// <remarks>
/// Implement an algorithm that determines two principal directions for the
/// supplied polygon and calculates an appropriate origin.
///
/// The method must return:
///
/// - Origin: the centre of the polygon's oriented bounding rectangle.
/// - U1: a normalised vector representing the dominant polygon direction.
/// - U2: a normalised vector perpendicular to U1.
///
/// Required properties:
///
/// - U1 and U2 must have approximately unit length.
/// - U1 and U2 must be approximately perpendicular.
/// - The result must be deterministic for the same input.
/// - The result should be independent of the polygon's absolute position.
/// - Invalid or degenerate input should be handled explicitly.
///
/// You may choose an appropriate method. Potential approaches include:
///
/// - Principal component analysis using the point covariance matrix.
/// - The direction between the most distant pair of points.
/// - The longest polygon edge.
/// - Convex hull analysis.
/// - A minimum-area bounding rectangle.
///
/// These methods have different levels of accuracy and complexity. A
/// minimum-area solution is not required.
///
/// Guidance:
///
/// 1. Identify a representative centre for the supplied points.
///
/// 2. Determine a dominant direction from the polygon geometry.
///
/// 3. Normalise the dominant direction to produce U1.
///
/// 4. Construct U2 as a perpendicular vector.
///
/// 5. Project every point into the coordinate system defined by U1 and U2.
///
/// 6. Find the minimum and maximum projection along each local axis.
///
/// 7. Use those projection ranges to reconstruct the centre of the oriented
///    bounds in world coordinates.
///
/// Degenerate cases to consider:
///
/// - Fewer than three points.
/// - All points being coincident.
/// - All points being collinear.
/// - Repeated points.
/// - A polygon with equal extents in several directions.
///
/// Where multiple mathematically equivalent vector directions are possible,
/// apply a consistent orientation rule so the result remains deterministic.
///
/// Be prepared to explain:
///
/// - What "dominant direction" means in your implementation.
/// - Why you selected the algorithm.
/// - Its computational complexity.
/// - Situations where it produces a poor approximation.
/// - How degenerate geometry is handled.
/// </remarks>
/// 

public sealed class OrientedBoundingBoxService : IOrientedBoundingBoxService
{
    private const double Epsilon = 1e-9;

    // Solution: Implementing the Longest Polygon Edge Method.
    // To calculate the origin of a polygon:
    private Point2Dto CalculateCentroid(List<Point2Dto> points)
    {
        double x = 0;
        double y = 0;

        foreach (var p in points)
        {
            x += p.X;
            y += p.Y;
        }

        return new Point2Dto(
            x / points.Count,
            y / points.Count
        );
    }

    // To find the longest edge of a polygon, we can iterate through the list of points and calculate the distance between each pair of consecutive points.
    // We will keep track of the longest edge found during this iteration.
    // The longest edge will be represented as an Edge2Dto object, which contains the start point, end point, and length of the edge.
    private Edge2Dto FindLongestEdge(List<Vector2Dto> points)
    {
        Edge2Dto longest = null;

        for (int i = 0; i < points.Count; i++)
        {
            var start = points[i];

            var end = points[(i + 1) % points.Count];

            var length = Distance(start, end);

            var edge = new Edge2Dto(
                start,
                end,
                length
            );

            if (longest == null || edge.Length > longest.Length)
            {
                longest = edge;
            }
        }

        return longest;
    }

    private double Distance(Vector2Dto a, Vector2Dto b) // a(X,Y), b(X, Y)
    {
        // Calculate the Euclidean distance between two points
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;

        return Math.Sqrt(
            dx * dx +
            dy * dy
        );
    }

    // Normalize a vector to unit length by dividing each component by the vector's length.
    private Vector2Dto Normalize(Vector2Dto vector)
    {
        var length = Math.Sqrt(
            vector.X * vector.X +
            vector.Y * vector.Y
        );


        return new Vector2Dto(
            vector.X / length,
            vector.Y / length
        );
    }

    // Verification of the dot product of two vectors to ensure they are perpendicular.
    // The dot product of two perpendicular vectors should be zero.
    // If the absolute value of the dot product is greater than a small epsilon value,
    // an exception is thrown indicating that the vectors are not perpendicular.
    private static void VerifyDot(Point2Dto u1, Point2Dto u2)
    {
        var dot = u1.X * u2.X + u1.Y * u2.Y;
        if (Math.Abs(dot) > Epsilon)
        {
            throw new InvalidOperationException(
                "Vectors are not perpendicular."
            );
        }

    }

    //public OrientedBoundingBoxResult Calculate(
    //    IReadOnlyCollection<Point2Dto> points)
    //{
    //    var origin = CalculateCentroid((List<Point2Dto>)points);

    //    var longestEdge = FindLongestEdge((List<Vector2Dto>)points);

    //    var u1 = Normalize(
    //        new Vector2Dto(
    //            longestEdge.End.X - longestEdge.Start.X,
    //            longestEdge.End.Y - longestEdge.Start.Y
    //        ));

    //    var u2 = new Vector2Dto(
    //        -u1.Y, // Perpendicular vector to u1
    //        u1.X
    //    );

    //    return new OrientedBoundingBoxResult(
    //        origin,
    //        u1,
    //        u2
    //    );

    //    throw new NotImplementedException();
    //}

    // C#
    public OrientedBoundingBoxResult Calculate(IReadOnlyCollection<Point2Dto> points)
    {
        var origin = CalculateCentroid(points.ToList()); // safer than direct cast

        var vectorPoints = points
            .Select(p => new Vector2Dto(p.X, p.Y))
            .ToList();

        var longestEdge = FindLongestEdge(vectorPoints);

        var u1 = Normalize(new Vector2Dto(
            longestEdge.End.X - longestEdge.Start.X,
            longestEdge.End.Y - longestEdge.Start.Y));

        var u2 = new Vector2Dto(-u1.Y, u1.X);

        return new OrientedBoundingBoxResult(origin, u1, u2);
    }
}