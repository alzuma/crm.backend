using Crm.Backend.Dtos;

namespace Crm.Backend.Application;

/// <summary>Application/service layer contract for the sample resource.
/// Business behaviour lives here; it depends on persistence, never on
/// endpoint types.</summary>
public interface ISampleService
{
    Task<SampleDto> CreateAsync(CreateSampleRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<SampleDto>> ListAsync(CancellationToken cancellationToken);
}
