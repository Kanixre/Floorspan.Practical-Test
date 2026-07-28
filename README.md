# Floorspan Practical Assessment

This repository contains a deliberately small .NET 8 application for creating, analysing, storing and displaying two-dimensional polygons.

The solution includes:

* An interactive Blazor page.
* An in-project ASP.NET Core Web API.
* A typed API client used by the Blazor UI.
* Entity Framework Core with SQLite.
* API contracts and persistence entities.
* Dependency-injection registration.
* A partially completed application structure.

Your task is to complete the missing backend functionality.

The assessment focuses on three main areas:

1. Standard database access using Entity Framework Core.
2. Geometric problem-solving.
3. API controller implementation and separation of responsibilities.

---

# Assessment objective

The application allows a user to submit an ordered collection of two-dimensional points representing a polygon.

When a polygon is submitted, the backend must:

1. Validate the supplied points.
2. determine the polygon's primary and secondary directions;
3. calculate an origin for its oriented bounds;
4. store the polygon, its ordered points and its calculated geometry;
5. return the created polygon through the API; and
6. make stored polygons available through the existing user interface.

The project shell already contains the contracts, entities, database context and application wiring required to support this workflow.

Several classes have deliberately been left incomplete. You are expected to locate and implement them.

---

# Required work

You are required to complete the following areas:

* `PolygonDbService`
* `OrientedBoundingBoxService`
* `PolygonService`
* `PolygonsController`

The database and geometry services are the primary implementation tasks.

The application service and controller should coordinate those components and expose the completed behaviour through the existing API.

Do not replace the existing architecture with a fundamentally different application structure.

You may introduce small supporting types or private helper methods where they improve clarity, but the provided abstractions should remain recognisable.

---

# Project workflow

The intended request flow is:

```text
Blazor page
    |
    v
Typed API client
    |
    v
PolygonsController
    |
    v
PolygonService
    |
    +---------------------------+
    |                           |
    v                           v
PolygonDbService      OrientedBoundingBoxService
    |                           |
    v                           v
AppDbContext             Geometry result
    |
    v
SQLite
```

Each layer has a separate responsibility.

## Controller

The controller is responsible for HTTP concerns:

* receiving requests;
* passing work to the application service;
* returning appropriate status codes;
* returning response bodies;
* translating expected validation failures into suitable client responses.

The controller should not:

* query Entity Framework Core directly;
* construct database entities;
* calculate geometry;
* contain the main application workflow.

## Polygon application service

`PolygonService` coordinates the use case.

It is responsible for:

* validating polygon requests;
* passing point data to the geometry service;
* constructing the persistence model;
* passing entities to the database service;
* mapping persistence entities into API contracts.

The application service should not:

* return `IActionResult`;
* decide HTTP status codes;
* directly perform EF Core queries;
* contain the oriented-bounding-box algorithm.

## Database service

`PolygonDbService` owns EF Core-specific persistence behaviour.

It is responsible for:

* querying stored polygons;
* loading their child point records;
* inserting new polygons;
* saving child point entities;
* preserving the point sequence;
* using asynchronous EF Core operations.

The database service should not:

* calculate geometry;
* validate HTTP requests;
* return API responses;
* contain controller logic.

## Geometry service

`OrientedBoundingBoxService` is responsible only for the geometric calculation.

It receives a collection of two-dimensional points and returns:

* an origin;
* a primary direction vector, `U1`;
* a secondary direction vector, `U2`.

The geometry service should not:

* access the database;
* create persistence entities;
* return HTTP responses;
* depend on the controller or Blazor UI.

---

# Where to work

The main incomplete files are located in the following areas.

## 1. Polygon database service

Locate:

```text
Services/Polygons/PolygonDbService.cs
```

You will also need to inspect:

```text
Services/Polygons/IPolygonDbService.cs
Data/AppDbContext.cs
Data/Entities/PolygonEntity.cs
Data/Entities/PolygonPointEntity.cs
```

The database service must support:

```csharp
Task<IReadOnlyList<PolygonEntity>> GetAllAsync(
    CancellationToken cancellationToken = default);

Task<PolygonEntity> CreateAsync(
    PolygonEntity entity,
    CancellationToken cancellationToken = default);
```

### Retrieval requirements

The retrieval operation should:

* load all stored polygons;
* include each polygon's associated points;
* use asynchronous EF Core APIs;
* pass the supplied cancellation token to EF Core;
* avoid unnecessary tracking for a read-only operation;
* provide enough data for the application layer to reconstruct the original point order.

The order of the polygons returned to the user should be deterministic.

You may implement ordering in the database service or application service, but you should be prepared to explain the choice.

### Creation requirements

The creation operation should:

* accept a fully constructed `PolygonEntity`;
* add it to the database context;
* persist the polygon and its child point entities;
* use `SaveChangesAsync`;
* pass through the cancellation token;
* return the stored entity.

You should inspect the entity relationships before implementing this method.

Consider how EF Core inserts related child entities when they are attached to a new parent entity.

---

## 2. Geometry service

Locate:

```text
Services/Geometry/OrientedBoundingBoxService.cs
```

You will also need to inspect:

```text
Services/Geometry/IOrientedBoundingBoxService.cs
Contracts/Geometry/Point2Dto.cs
Contracts/Geometry/Vector2Dto.cs
Contracts/Geometry/OrientedBoundingBoxResult.cs
```

The geometry service must implement:

```csharp
OrientedBoundingBoxResult Calculate(
    IReadOnlyCollection<Point2Dto> points);
```

This is the main problem-solving portion of the assessment.

---

# Geometry requirement

Given a collection of points representing a polygon, determine an approximate local coordinate system for that polygon.

The result must contain:

```csharp
Origin
U1
U2
```

## Primary vector

`U1` should represent the polygon's dominant or primary direction.

There is more than one reasonable way to define a polygon's primary direction.

Possible approaches include:

* analysing the covariance of the supplied points;
* using the longest polygon edge;
* finding the most distant pair of points;
* analysing the polygon's convex hull;
* calculating a minimum-area bounding rectangle;
* another clearly justified geometric method.

A minimum-area bounding rectangle is not required.

Your implementation should be appropriate for the size and scope of the assessment. You should be able to explain why you selected it and where it may produce imperfect results.

## Secondary vector

`U2` should represent the secondary direction of the polygon.

It should be perpendicular to `U1`.

Both vectors should be normalised:

```text
length(U1) ≈ 1
length(U2) ≈ 1
```

They should also satisfy:

```text
dot(U1, U2) ≈ 0
```

Floating-point calculations should be compared using a reasonable tolerance rather than exact equality.

## Origin

For this assessment, the origin should represent the centre of the polygon's approximate oriented bounds.

One general way to determine this is to:

1. select a representative centre for the input points;
2. determine `U1` and `U2`;
3. project every point onto those two axes;
4. find the minimum and maximum projection along each axis;
5. calculate the midpoint of each projection range; and
6. convert that local midpoint back into world coordinates.

You are not required to use this exact sequence if your chosen method produces an equivalent and defensible result.

Do not simply return the first supplied point as the origin.

---

# Geometry edge cases

Your implementation should explicitly consider invalid or degenerate geometry.

At minimum, consider:

* fewer than three points;
* fewer than three distinct points;
* coincident points;
* non-finite coordinates;
* collinear points;
* repeated points;
* shapes whose primary direction is ambiguous.

You do not need to solve every possible computational-geometry edge case.

However, your implementation should not silently return invalid vectors containing:

```text
NaN
Infinity
zero-length directions
```

Where the geometry cannot produce a meaningful result, return or throw an appropriate error that can be handled by the application layer.

## Deterministic vector orientation

Direction vectors are sign-ambiguous.

For example, these vectors describe the same axis:

```text
(0.8, 0.6)
(-0.8, -0.6)
```

Your implementation should apply a consistent orientation rule so that the same polygon produces the same result across repeated calls.

You may define your own deterministic rule.

For example, you might prefer a primary vector that points towards positive X where possible, falling back to positive Y for a near-vertical axis.

Be prepared to explain the rule you selected.

---

# Polygon application service

Locate:

```text
Services/Polygons/PolygonService.cs
```

You will also need to inspect:

```text
Services/Polygons/IPolygonService.cs
Services/Polygons/IPolygonDbService.cs
Services/Geometry/IOrientedBoundingBoxService.cs
Contracts/Polygons/CreatePolygonRequest.cs
Contracts/Polygons/PolygonDto.cs
Data/Entities/PolygonEntity.cs
Data/Entities/PolygonPointEntity.cs
```

The application service exposes:

```csharp
Task<IReadOnlyList<PolygonDto>> GetAllAsync(
    CancellationToken cancellationToken = default);

Task<PolygonDto> CreateAsync(
    CreatePolygonRequest request,
    CancellationToken cancellationToken = default);
```

## `GetAllAsync`

This method should:

1. retrieve persisted polygons through `IPolygonDbService`;
2. map each entity into a `PolygonDto`;
3. restore the original order of each polygon's points;
4. return the polygons in a deterministic order.

Do not inject or access `AppDbContext` directly from this service.

## `CreateAsync`

This method should implement the complete polygon-creation workflow.

It should:

1. validate the request;
2. materialise the supplied point collection;
3. pass the points to the geometry service;
4. create a new polygon entity;
5. assign a new identifier;
6. assign a UTC creation timestamp;
7. store the calculated origin and direction vectors;
8. create child point entities;
9. preserve the order in which the points were submitted;
10. pass the completed entity to the database service;
11. return the stored polygon as a `PolygonDto`.

The application service is responsible for coordinating the operation, not for performing database queries or implementing geometric mathematics itself.

---

# Validation requirements

At minimum, polygon creation must reject:

* a null request;
* fewer than three points;
* fewer than three distinct points;
* coordinates containing `NaN`;
* coordinates containing positive or negative infinity.

You may implement additional validation, including:

* rejection of consecutive duplicate points;
* rejection of a repeated closing point;
* rejection of zero-area polygons;
* rejection of entirely collinear points;
* self-intersection detection.

Additional validation is optional.

Do not spend the entire assessment implementing advanced polygon validation at the expense of the required controller, database and geometry work.

Validation failures should produce meaningful error messages that can be translated into an appropriate API response.

---

# Entity mapping

The persistence model stores geometry as scalar database columns.

The API exposes geometry through structured contracts.

The application service must map between these representations.

A returned `PolygonDto` should contain:

```text
Id
CreatedUtc
Points
Origin
U1
U2
```

The stored point sequence must be respected when constructing the DTO.

Do not assume that a database query will automatically return child records in insertion order.

Use the sequence field provided by the persistence model.

The database entity may allow nullable geometry values. Consider how those values should be represented in the API contract.

---

# API controller

Locate:

```text
Controllers/PolygonsController.cs
```

You will also need to inspect:

```text
Services/Polygons/IPolygonService.cs
Contracts/Polygons/CreatePolygonRequest.cs
Contracts/Polygons/PolygonDto.cs
```

The API exposes:

```http
GET /api/polygons
POST /api/polygons
```

## `GET /api/polygons`

This endpoint should:

* call `IPolygonService.GetAllAsync`;
* pass through the request cancellation token;
* return HTTP `200 OK`;
* return an empty collection when no polygons exist.

An empty database is not an error.

## `POST /api/polygons`

This endpoint should:

* accept a JSON `CreatePolygonRequest`;
* delegate processing to `IPolygonService`;
* pass through the request cancellation token;
* return the created polygon;
* return HTTP `201 Created` for a successful request;
* return HTTP `400 Bad Request` for expected validation failures.

The error response should contain useful information for the caller.

The controller should not expose stack traces or internal implementation details.

You may use ASP.NET Core `ProblemDetails` for client-error responses.

---

# Example API request

```http
POST /api/polygons
Content-Type: application/json
```

```json
{
  "points": [
    { "x": 0, "y": 0 },
    { "x": 240, "y": 40 },
    { "x": 180, "y": 180 },
    { "x": 20, "y": 140 }
  ]
}
```

A successful request should:

1. validate the four supplied points;
2. calculate an approximate oriented basis;
3. calculate the centre of the oriented bounds;
4. store the polygon;
5. store the points in the submitted order;
6. store the calculated geometry;
7. return the created polygon.

The created polygon should then appear in:

```http
GET /api/polygons
```

and in the existing Blazor user interface.

---

# Running the project

The assessment requires the .NET 8 SDK.

From the project directory, run:

```bash
dotnet restore
dotnet run
```

Open the URL printed by ASP.NET Core.

The development URLs will normally resemble:

```text
https://localhost:7198
http://localhost:5198
```

The exact ports may vary depending on the local environment.

---

# SQLite database

The application uses Entity Framework Core with SQLite.

On startup, the application calls:

```text
Database.EnsureCreatedAsync()
```

The database is created automatically on first run.

The generated file is:

```text
practical-assessment.db
```

It will normally appear in the project working directory.

You do not need to create an EF Core migration for this assessment.

`EnsureCreatedAsync` is intentionally used because this is a disposable assessment project.

In a production application, schema changes would normally be managed through EF Core migrations.

---

# Resetting the application

To reset the local data, stop the application and delete:

```text
practical-assessment.db
practical-assessment.db-shm
practical-assessment.db-wal
```

The application will create a fresh database the next time it starts.

These generated database files should not be committed to source control.

---

# Dependency injection

The required interfaces and implementations should be registered in the application's dependency-injection container.

Inspect:

```text
Program.cs
```

Confirm that the following abstractions resolve successfully:

```text
IPolygonService
IPolygonDbService
IOrientedBoundingBoxService
```

Do not instantiate these services manually inside the controller.

The application should start without dependency-resolution errors.

---

# Expected separation of responsibilities

A successful implementation should preserve the following boundaries.

| Component                    | Responsibility                                  |
| ---------------------------- | ----------------------------------------------- |
| `PolygonsController`         | HTTP request and response handling              |
| `PolygonService`             | Validation, coordination and entity/DTO mapping |
| `PolygonDbService`           | EF Core queries and persistence                 |
| `OrientedBoundingBoxService` | Geometric calculation                           |
| `AppDbContext`               | EF Core database context                        |
| Contracts                    | API request and response structures             |
| Entities                     | SQLite persistence structures                   |

The assessment is not looking for the largest possible architecture.

Prefer a small, clear implementation over unnecessary abstractions.

---

# Constraints

You must:

* use .NET 8;
* use the existing project structure;
* retain SQLite;
* use the supplied `AppDbContext`;
* use asynchronous EF Core operations;
* pass cancellation tokens through asynchronous calls;
* preserve the submitted point order;
* keep database work out of the controller;
* keep geometry work out of the controller and application service;
* return meaningful HTTP responses;
* produce deterministic geometry results.

You should not:

* replace EF Core with raw SQL;
* replace SQLite with another database;
* move the entire application into the controller;
* use static global state;
* hard-code the result for the example polygon;
* add large external geometry libraries;
* add unnecessary packages;
* rewrite the Blazor interface unless required to make the completed workflow function;
* commit generated database files.

Small helper methods and supporting types are acceptable.

---

# Testing your implementation

Before submitting, verify the following manually.

## Application startup

```text
The project restores successfully.
The project builds successfully.
The application starts without exceptions.
The SQLite database is created.
The Blazor page loads.
```

## Polygon creation

```text
A valid polygon can be submitted.
The API returns 201 Created.
The returned polygon contains an ID.
The returned polygon contains a UTC creation timestamp.
The returned polygon contains Origin, U1 and U2.
The returned points remain in the submitted order.
```

## Polygon retrieval

```text
GET /api/polygons returns 200 OK.
The newly created polygon appears in the response.
An empty database returns an empty collection rather than an error.
```

## Validation

Verify that invalid requests are rejected, including:

```text
No points.
One point.
Two points.
Three identical points.
Coordinates containing invalid floating-point values where applicable.
```

## Geometry

Verify that:

```text
U1 has approximately unit length.
U2 has approximately unit length.
U1 and U2 are approximately perpendicular.
The result does not contain NaN or Infinity.
The same input returns the same vector orientation.
Translating all polygon points does not change the direction vectors.
```

---

# Automated tests

Automated tests are not mandatory unless specifically requested during the assessment.

However, focused tests may strengthen your submission.

Useful test areas include:

* polygon validation;
* entity-to-DTO mapping;
* point sequence preservation;
* geometry vector normalisation;
* vector orthogonality;
* deterministic vector orientation;
* translated copies of the same polygon;
* coincident or collinear input;
* API integration using an in-memory or temporary SQLite database.

Do not prioritise a large test suite over completing the required implementation.

---

# Evaluation criteria

Your submission will be assessed primarily on:

## Database implementation

* correct EF Core usage;
* asynchronous operations;
* loading related point records;
* preserving point order;
* sensible tracking behaviour;
* appropriate cancellation-token propagation.

## Geometry implementation

* clarity of the selected method;
* mathematical correctness;
* normalised and perpendicular vectors;
* meaningful origin calculation;
* deterministic results;
* handling of degenerate inputs;
* ability to explain limitations.

## Application structure

* clear separation of responsibilities;
* correct use of dependency injection;
* appropriate coordination between services;
* understandable mapping between entities and contracts;
* avoidance of unnecessary complexity.

## API behaviour

* correct HTTP status codes;
* useful successful responses;
* useful validation responses;
* no persistence or geometry logic in the controller.

## Code quality

* readable naming;
* small, focused methods;
* reasonable error handling;
* consistency with the existing codebase;
* comments where the reasoning is not obvious;
* no unexplained generated or unnecessary code.

---

# Review discussion

You should be prepared to walk through your implementation.

The review may include questions such as:

* How did you define the polygon's primary direction?
* Why did you choose that algorithm?
* What is the algorithm's time complexity?
* When might it produce a poor approximation?
* How did you make the vector direction deterministic?
* What happens when all points are collinear?
* Why does the database query use or avoid tracking?
* How is point order preserved?
* Why does validation belong in the selected layer?
* Why should the controller not access `AppDbContext`?
* What would you change for a production application?
* How would you test the geometry without relying on exact floating-point values?

You may also be asked to make a small change to the completed solution during the review.

---

# Optional extensions

Only attempt optional work after the required functionality is complete.

Possible extensions include:

* unit tests;
* API integration tests;
* improved validation responses;
* polygon area validation;
* self-intersection detection;
* update and delete endpoints;
* a true minimum-area oriented bounding rectangle;
* complete oriented-box rendering in the UI;
* optimistic UI updates;
* database migrations;
* structured application exceptions;
* improved logging.

Optional work will not compensate for incomplete required functionality.

---

# Submission expectation

Submit a solution that:

* builds;
* runs;
* creates its own SQLite database;
* accepts valid polygon requests;
* rejects clearly invalid requests;
* calculates a defensible oriented basis;
* stores polygons and ordered points;
* retrieves stored polygons;
* keeps the controller, application, database and geometry responsibilities separate.

The objective is not to produce a production-ready computational-geometry library.

The objective is to demonstrate that you can implement a small vertical slice involving:

* ASP.NET Core;
* dependency injection;
* asynchronous EF Core persistence;
* request validation;
* entity and DTO mapping;
* basic computational geometry;
* clear architectural boundaries;
* reasoned technical decision-making.
