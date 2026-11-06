namespace Crm.Backend.Persistence.Entities;

/// <summary>A recorded interaction (call, meeting, note). Always belongs to
/// a company; the contact and the opportunity are optional links.</summary>
public class Activity
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }

    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }

    public string Type { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }

    public string? Summary { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
