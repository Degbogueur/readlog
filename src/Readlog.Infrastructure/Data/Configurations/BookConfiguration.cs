using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Readlog.Domain.Entities;
using Readlog.Domain.ValueObjects;

namespace Readlog.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Isbn)
            .HasConversion(
                isbn => isbn != null ? isbn.Value : null,
                value => value != null ? ISBN.CreateOrDefault(value) : null)
            .HasMaxLength(13);

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        builder.Property(b => b.PublishedDate)
            .HasColumnType("date");

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.CreatedBy)
            .IsRequired();

        builder.HasIndex(b => b.CreatedBy);
    }
}
