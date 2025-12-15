using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Readlog.Domain.Entities;
using Readlog.Domain.ValueObjects;

namespace Readlog.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.BookId)
            .IsRequired();

        builder.Property(r => r.Rating)
            .HasConversion(
                rating => rating.Value,
                value => Rating.Create(value))
            .IsRequired();

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .IsRequired();

        builder.HasIndex(r => r.BookId);
        builder.HasIndex(r => r.CreatedBy);
        builder.HasIndex(r => new { r.BookId, r.CreatedBy }).IsUnique();
    }
}
