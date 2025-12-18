using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Readlog.Domain.Entities;

namespace Readlog.Infrastructure.Data.Configurations;

public class ReadingListItemConfiguration : IEntityTypeConfiguration<ReadingListItem>
{
    public void Configure(EntityTypeBuilder<ReadingListItem> builder)
    {
        builder.ToTable("ReadingListItems");

        builder.HasKey(rli => rli.Id);

        builder.Property(rli => rli.Id)
            .ValueGeneratedNever();

        builder.Property(rli => rli.ReadingListId)
            .IsRequired();

        builder.Property(rli => rli.BookId)
            .IsRequired();

        builder.Property(rli => rli.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(rli => rli.AddedAt)
            .IsRequired();

        builder.HasIndex(rli => new { rli.ReadingListId, rli.BookId })
            .IsUnique();
    }
}