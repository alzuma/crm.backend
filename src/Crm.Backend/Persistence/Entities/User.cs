namespace Crm.Backend.Persistence.Entities;

/// <summary>A user of the system. Email is unique; uniqueness applies to the
/// normalized value (see NormalizedEmail) enforced by the database.</summary>
public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    /// <summary>UPPER(TRIM(Email)), maintained by the database. Never written
    /// by the application; the unique index on it is the dedupe rule.</summary>
    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Role name (Administrator / Sales User values are introduced
    /// with authentication, CRM-3).</summary>
    public string? Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ICollection<Opportunity> OwnedOpportunities { get; set; } = new List<Opportunity>();
    public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
}
