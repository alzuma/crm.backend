using System.Collections.Concurrent;

namespace Crm.Backend.Persistence;

/// <summary>Stand-in persistence: an in-memory store behind the repository
/// contract. Replaced by the SQLite/EF Core implementation in CRM-7; no
/// database is required to run the scaffold.</summary>
public sealed class InMemorySampleRepository : ISampleRepository
{
    private readonly ConcurrentDictionary<Guid, Sample> _samples = new();

    public Task AddAsync(Sample sample, CancellationToken cancellationToken)
    {
        _samples[sample.Id] = sample;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Sample>> ListAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<Sample>>(_samples.Values.OrderBy(static s => s.CreatedAtUtc).ToList());
}
