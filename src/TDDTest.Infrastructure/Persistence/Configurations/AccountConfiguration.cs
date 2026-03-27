using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Enums;

namespace TDDTest.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.AccountNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(a => a.AccountNumber)
            .IsUnique();

        builder.Property(a => a.OwnerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(a => a.Email);

        builder.Property(a => a.Balance)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);

        builder.HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Access private backing field for the navigation collection
        builder.Navigation(a => a.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_transactions");
    }
}
