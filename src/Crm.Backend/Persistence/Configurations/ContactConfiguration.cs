using Crm.Backend.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Backend.Persistence.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName).HasMaxLength(100);
        builder.Property(c => c.LastName).HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(320);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.JobTitle).HasMaxLength(100);
        builder.Property(c => c.Notes).HasMaxLength(4000);

        builder
            .HasOne(c => c.Company)
            .WithMany(company => company.Contacts)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign key index: contacts are listed per company.
        builder.HasIndex(c => c.CompanyId);
    }
}
