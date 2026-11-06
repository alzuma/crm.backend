namespace Crm.Backend.Persistence;

/// <summary>Persistence layer contract. The application layer depends on this
/// interface; the endpoint layer never does.</summary>
public interface ISampleRepository
{
    Task AddAsync(Sample sample, CancellationToken cancellationToken);

    Task<IReadOnlyList<Sample>> ListAsync(CancellationToken cancellationToken);
}
