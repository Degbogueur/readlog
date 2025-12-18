using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Readlog.Domain.Entities;

namespace Readlog.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(150);

        builder.OwnsOne(b => b.Isbn, owned =>
        {
            owned.Property(i => i.Value)
                .HasColumnName("Isbn")
                .HasMaxLength(13);
        });

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        builder.Property(b => b.PublishedDate)
            .HasColumnType("date");

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.CreatedBy)
            .IsRequired();

        builder.Property(b => b.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(b => !b.IsDeleted);

        builder.HasIndex(b => b.CreatedBy);
        builder.HasIndex(b => b.IsDeleted);
    }
}
