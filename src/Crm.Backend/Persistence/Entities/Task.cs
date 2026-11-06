namespace Crm.Backend.Persistence.Entities;

/// <summary>A follow-up task. Always assigned to exactly one user; the
/// company and the opportunity are optional links.</summary>
public class Task
{
    public Guid Id { get; set; }

    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }

    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }

    public Guid AssignedTo { get; set; }
    public User AssignedToUser { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime? DueAt { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
