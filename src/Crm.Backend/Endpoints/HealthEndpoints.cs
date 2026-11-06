namespace Crm.Backend.Endpoints;

/// <summary>Liveness probe. Answers without touching any dependency, so it
/// works even when nothing else in the system is configured yet.</summary>
public static class HealthEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/health/live", () => Results.Ok(new { status = "OK" }))
            .WithName("HealthLive");
    }
}
