using Crm.Backend.Dtos;
using Crm.Backend.Persistence;

namespace Crm.Backend.Application;

/// <summary>Application/service layer for the sample resource: holds the
/// behaviour, reads and writes data only through the persistence layer.</summary>
public sealed class SampleService(ISampleRepository repository) : ISampleService
{
    public async Task<SampleDto> CreateAsync(CreateSampleRequest request, CancellationToken cancellationToken)
    {
        var sample = new Sample(
            Id: Guid.NewGuid(),
            Name: request.Name,
            Quantity: request.Quantity,
            CreatedAtUtc: DateTime.UtcNow);
        await repository.AddAsync(sample, cancellationToken);
        return sample.ToDto();
    }

    public async Task<IReadOnlyList<SampleDto>> ListAsync(CancellationToken cancellationToken)
    {
        var samples = await repository.ListAsync(cancellationToken);
        return samples.Select(static sample => sample.ToDto()).ToList();
    }
}
