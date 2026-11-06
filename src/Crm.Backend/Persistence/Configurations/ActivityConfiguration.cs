using Crm.Backend.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Backend.Persistence.Configurations;

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type).HasMaxLength(50);
        builder.Property(a => a.Summary).HasMaxLength(500);
        builder.Property(a => a.Notes).HasMaxLength(4000);

        builder
            .HasOne(a => a.Company)
            .WithMany(company => company.Activities)
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Optional links: an activity may record only a company.
        builder
            .HasOne(a => a.Contact)
            .WithMany(contact => contact.Activities)
            .HasForeignKey(a => a.ContactId);

        builder
            .HasOne(a => a.Opportunity)
            .WithMany(opportunity => opportunity.Activities)
            .HasForeignKey(a => a.OpportunityId);

        // Foreign key indexes plus the common filter on activity type.
        builder.HasIndex(a => a.CompanyId);
        builder.HasIndex(a => a.ContactId);
        builder.HasIndex(a => a.OpportunityId);
        builder.HasIndex(a => a.Type);
    }
}
