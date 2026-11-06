using Crm.Backend.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Backend.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200);
        builder.Property(c => c.Website).HasMaxLength(500);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Industry).HasMaxLength(100);
        builder.Property(c => c.Status).HasMaxLength(50);
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.Notes).HasMaxLength(4000);

        // Company name must be non-empty (whitespace-only counts as empty).
        // TRIM with an explicit character set covers spaces, tabs and line
        // breaks, not just U+0020.
        builder.HasCheckConstraint(
            "Company_Name_NonEmpty",
            "TRIM(Name, ' ' || CHAR(9) || CHAR(10) || CHAR(13)) <> ''");

        // Common filter: listing and filtering companies by status.
        builder.HasIndex(c => c.Status);
    }
}
