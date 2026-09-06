using Floorspan.PracticalAssessment.Shell.Contracts.Polygons;
using Floorspan.PracticalAssessment.Shell.Services.Polygons;
using Microsoft.AspNetCore.Mvc;
using static Floorspan.PracticalAssessment.Shell.Services.Polygons.PolygonService;

namespace Floorspan.PracticalAssessment.Shell.Controllers;

/// <summary>
/// Provides HTTP endpoints for creating and retrieving polygons.
///
/// Complete this controller using the existing IPolygonService abstraction.
/// The controller should remain focused on HTTP concerns and should not contain
/// database-access or geometry-calculation logic.
/// </summary>
[ApiController]
[Route("api/polygons")]
public sealed class PolygonsController(IPolygonService polygonService) : ControllerBase
{
    /// <summary>
    /// Returns all polygons currently stored by the application.
    /// </summary>
    /// <remarks>
    /// Requirements:
    ///
    /// - Call the appropriate method on IPolygonService.
    /// - Pass the supplied cancellation token through to the service.
    /// - Return a successful HTTP response containing the polygons.
    /// - Do not access AppDbContext directly from this controller.
    ///
    /// Consider what HTTP status code is appropriate for a successful request,
    /// including when the database contains no polygons.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PolygonDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PolygonDto>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var polygons = await polygonService.GetAllAsync(cancellationToken);

        return Ok(polygons);
    }

    /// <summary>
    /// Validates, calculates and stores a new polygon.
    /// </summary>
    /// <remarks>
    /// Requirements:
    ///
    /// - Accept a CreatePolygonRequest from the request body.
    /// - Delegate polygon processing to IPolygonService.
    /// - Pass the supplied cancellation token through to the service.
    /// - Return the created PolygonDto when successful.
    /// - Return an appropriate client-error response when the polygon is invalid.
    /// - Include useful error information without exposing internal implementation details.
    ///
    /// The controller should translate application-level failures into suitable
    /// HTTP responses. It should not perform the geometry calculation or persist
    /// entities itself.
    ///
    /// Consider:
    ///
    /// - Which status code best represents successful creation?
    /// - Which status code should be returned for invalid coordinates or too few points?
    /// - How should validation failures be represented consistently?
    /// </remarks>
    [HttpPost]
    [ProducesResponseType<PolygonDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PolygonDto>> CreateAsync(
        [FromBody] CreatePolygonRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await polygonService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetAllAsync),
                new { id = created.Id },
                created);
        }
        catch (PolygonValidationException ex)
        {
            return Problem(
                title: "Invalid polygon input.",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
            }
        }
    }