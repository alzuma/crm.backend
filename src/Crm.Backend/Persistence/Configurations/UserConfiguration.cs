using Crm.Backend.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Backend.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(320);
        builder.Property(u => u.PasswordHash).HasMaxLength(512);
        builder.Property(u => u.DisplayName).HasMaxLength(200);
        builder.Property(u => u.Role).HasMaxLength(50);

        // Email uniqueness is enforced on the normalized value: upper case,
        // surrounding whitespace removed, computed and stored by the database.
        // TRIM with an explicit character set covers spaces, tabs and line
        // breaks, not just U+0020.
        builder
            .Property(u => u.NormalizedEmail)
            .HasComputedColumnSql("UPPER(TRIM(Email, ' ' || CHAR(9) || CHAR(10) || CHAR(13)))", stored: true);
        builder.HasIndex(u => u.NormalizedEmail).IsUnique();
    }
}
