using Floorspan.PracticalAssessment.Shell.Components;
using Floorspan.PracticalAssessment.Shell.Data;
using Floorspan.PracticalAssessment.Shell.Services.Api;
using Floorspan.PracticalAssessment.Shell.Services.Geometry;
using Floorspan.PracticalAssessment.Shell.Services.Polygons;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("AppDb")
        ?? "Data Source=practical-assessment.db"));

builder.Services.AddScoped<IPolygonApi, PolygonApi>();
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.AddScoped<IOrientedBoundingBoxService, OrientedBoundingBoxService>();

// Program.cs (near other service registrations)
builder.Services.AddScoped<IPolygonDbService, PolygonDbService>();
builder.Services.AddScoped<IPolygonApi, PolygonApi>();
builder.Services.AddScoped<IPolygonService, PolygonService>();
builder.Services.AddScoped<IOrientedBoundingBoxService, OrientedBoundingBoxService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("An unexpected server error occurred.");
        });
    });

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await EnsureDatabaseCreatedAsync(app);
await app.RunAsync();

static async Task EnsureDatabaseCreatedAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}