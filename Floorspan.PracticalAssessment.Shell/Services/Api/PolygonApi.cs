using Floorspan.PracticalAssessment.Shell.Contracts.Polygons;
using Microsoft.AspNetCore.Components;

namespace Floorspan.PracticalAssessment.Shell.Services.Api;

public sealed class PolygonApi(
    IHttpClientFactory httpClientFactory,
    NavigationManager navigationManager) : IPolygonApi
{
    public async Task<IReadOnlyList<PolygonDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();

        return await client.GetFromJsonAsync<PolygonDto[]>(
                   "api/polygons",
                   cancellationToken)
               ?? Array.Empty<PolygonDto>();
    }

    public async Task<PolygonDto> CreateAsync(
        CreatePolygonRequest request,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var response = await client.PostAsJsonAsync(
            "api/polygons",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PolygonDto>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(navigationManager.BaseUri);
        return client;
    }
}
