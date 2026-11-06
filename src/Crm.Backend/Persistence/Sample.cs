using Crm.Backend.Dtos;

namespace Crm.Backend.Persistence;

/// <summary>The stored sample record. This type belongs to the persistence
/// layer only; callers see it through SampleDto.</summary>
public sealed record Sample(Guid Id, string Name, int Quantity, DateTime CreatedAtUtc)
{
    public SampleDto ToDto() => new(Id, Name, Quantity, CreatedAtUtc);
}
