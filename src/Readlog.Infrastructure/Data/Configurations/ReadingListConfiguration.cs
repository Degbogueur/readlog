using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Readlog.Domain.Entities;

namespace Readlog.Infrastructure.Data.Configurations;

public class ReadingListConfiguration : IEntityTypeConfiguration<ReadingList>
{
    public void Configure(EntityTypeBuilder<ReadingList> builder)
    {
        builder.ToTable("ReadingLists");

        builder.HasKey(rl => rl.Id);

        builder.Property(rl => rl.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rl => rl.CreatedAt)
            .IsRequired();

        builder.Property(rl => rl.CreatedBy)
            .IsRequired();

        builder.Property(rl => rl.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(rl => !rl.IsDeleted);

        builder.HasMany(rl => rl.Items)
            .WithOne()
            .HasForeignKey(rli => rli.ReadingListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(rl => rl.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(rl => rl.CreatedBy);
        builder.HasIndex(rl => rl.IsDeleted);
    }
}
