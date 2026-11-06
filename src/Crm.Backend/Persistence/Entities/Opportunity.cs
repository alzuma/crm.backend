namespace Crm.Backend.Persistence.Entities;

/// <summary>A sales opportunity. Belongs to exactly one company and has
/// exactly one owner. Amount is integer minor units with an ISO 4217
/// currency code; probability is bounded 0-100 by the database.</summary>
public class Opportunity
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public int AmountMinor { get; set; }
    public string? Currency { get; set; }

    public OpportunityStage Stage { get; set; }

    public int Probability { get; set; }

    public DateTime? ExpectedCloseDate { get; set; }
    public string? LossReason { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}
