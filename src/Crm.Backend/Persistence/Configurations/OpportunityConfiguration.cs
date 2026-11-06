using Crm.Backend.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Backend.Persistence.Configurations;

public sealed class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title).HasMaxLength(200);
        builder.Property(o => o.Currency).HasMaxLength(3);
        builder.Property(o => o.LossReason).HasMaxLength(1000);
        builder.Property(o => o.Stage).HasConversion<string>().HasMaxLength(50);

        // Opportunity title must be non-empty (whitespace-only counts as empty).
        // TRIM with an explicit character set covers spaces, tabs and line
        // breaks, not just U+0020.
        builder.HasCheckConstraint(
            "Opportunity_Title_NonEmpty",
            "TRIM(Title, ' ' || CHAR(9) || CHAR(10) || CHAR(13)) <> ''");
        // Probability is bounded 0-100.
        builder.HasCheckConstraint("Opportunity_Probability_Bounded", "Probability >= 0 AND Probability <= 100");
        // Amount is integer minor units and nonnegative.
        builder.HasCheckConstraint("Opportunity_Amount_NonNegative", "AmountMinor >= 0");

        builder
            .HasOne(o => o.Company)
            .WithMany(company => company.Opportunities)
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(o => o.Owner)
            .WithMany(user => user.OwnedOpportunities)
            .HasForeignKey(o => o.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign key indexes.
        builder.HasIndex(o => o.CompanyId);
        builder.HasIndex(o => o.OwnerId);

        // Pipeline views filter by stage.
        builder.HasIndex(o => o.Stage);
    }
}
