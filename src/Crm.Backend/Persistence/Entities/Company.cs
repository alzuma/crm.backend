namespace Crm.Backend.Persistence.Entities;

/// <summary>A company (account). A company with dependent contacts,
/// opportunities, activities or tasks cannot be deleted: the foreign keys
/// block the delete until the dependent records are resolved.</summary>
public class Company
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Industry { get; set; }
    public string? Status { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}
