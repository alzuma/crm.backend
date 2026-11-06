using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Crm.Backend.Tests;

/// <summary>Integration tests against the real host: prove the request
/// pipeline, dependency injection and the endpoint to persistence flow.</summary>
public class SampleApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SampleApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthLive_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("OK", body?.Status);
    }

    [Fact]
    public async Task CreateSample_WithValidRequest_ReturnsCreatedWithDto()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/samples", new { name = "Acme", quantity = 3 });

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SampleResponse>();
        Assert.NotNull(body);
        Assert.Equal("Acme", body!.Name);
        Assert.Equal(3, body.Quantity);
        Assert.NotEqual(Guid.Empty, body.Id);
    }

    [Fact]
    public async Task CreateSample_WithWhitespaceName_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/samples", new { name = "   ", quantity = 3 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSample_WithMissingName_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/samples", new { quantity = 3 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSample_WithOutOfRangeQuantity_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/samples", new { name = "Acme", quantity = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListSamples_ReturnsPreviouslyCreatedSample()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/v1/samples", new { name = "Persisted", quantity = 5 });

        var response = await client.GetFromJsonAsync<List<SampleResponse>>("/api/v1/samples");

        Assert.Contains(response ?? new List<SampleResponse>(), sample => sample.Name == "Persisted" && sample.Quantity == 5);
    }

    private sealed record HealthResponse(string Status);

    private sealed record SampleResponse(Guid Id, string Name, int Quantity, DateTime CreatedAtUtc);
}
