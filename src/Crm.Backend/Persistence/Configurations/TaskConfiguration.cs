using Crm.Backend.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = Crm.Backend.Persistence.Entities.Task;

namespace Crm.Backend.Persistence.Configurations;

public sealed class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(4000);
        builder.Property(t => t.Priority).HasMaxLength(50);
        builder.Property(t => t.Status).HasMaxLength(50);

        // Deleting a company is blocked while tasks still reference it.
        builder
            .HasOne(t => t.Company)
            .WithMany(company => company.Tasks)
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(t => t.Opportunity)
            .WithMany(opportunity => opportunity.Tasks)
            .HasForeignKey(t => t.OpportunityId);

        builder
            .HasOne(t => t.AssignedToUser)
            .WithMany(user => user.AssignedTasks)
            .HasForeignKey(t => t.AssignedTo)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign key indexes, the common filter on status and the due-date
        // index for "tasks due" views.
        builder.HasIndex(t => t.CompanyId);
        builder.HasIndex(t => t.OpportunityId);
        builder.HasIndex(t => t.AssignedTo);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.DueAt);
    }
}
