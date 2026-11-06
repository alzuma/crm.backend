using Crm.Backend.Application;
using Crm.Backend.Dtos;
using Crm.Backend.Validation;

namespace Crm.Backend.Endpoints;

/// <summary>Sample resource: the visible example of the endpoint layer.
/// Endpoints bind and validate the request, then hand it to the application
/// service. They never touch persistence or business rules directly.</summary>
public static class SampleEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost("/api/v1/samples", async (CreateSampleRequest request, ISampleService service, CancellationToken cancellationToken) =>
            {
                if (!DtoValidator.TryValidate(request, out var errors))
                {
                    return Results.ValidationProblem(errors);
                }

                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/v1/samples/{created.Id}", created);
            })
            .WithName("CreateSample");

        endpoints
            .MapGet("/api/v1/samples", async (ISampleService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.ListAsync(cancellationToken)))
            .WithName("ListSamples");
    }
}
