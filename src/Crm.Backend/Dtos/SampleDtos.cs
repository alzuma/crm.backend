using System.ComponentModel.DataAnnotations;
using Crm.Backend.Validation;

namespace Crm.Backend.Dtos;

/// <summary>Incoming data for creating a sample. Data annotations on the DTO
/// are the designated validation example: the endpoint layer runs them
/// through DtoValidator and rejects invalid requests with 400 before the
/// service is called.</summary>
public sealed record CreateSampleRequest
{
    [Required]
    [NotWhitespace]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [Range(1, 1000)]
    public int Quantity { get; init; }
}

/// <summary>Outgoing sample representation. Records created by the persistence
/// layer are mapped to this shape so internal types never cross the boundary.</summary>
public sealed record SampleDto(Guid Id, string Name, int Quantity, DateTime CreatedAtUtc);
