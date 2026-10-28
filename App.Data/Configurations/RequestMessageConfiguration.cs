using App.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Data.Configurations;

public class RequestMessageConfiguration : IEntityTypeConfiguration<RequestMessage>
{
    public void Configure(EntityTypeBuilder<RequestMessage> builder)
    {
        builder.ToTable("RequestMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.AuthorName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.AuthorRole).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(2000);
        builder.Property(m => m.Visibility).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.CreatedAt).IsRequired();

        builder.HasOne(m => m.Author)
            .WithMany()
            .HasForeignKey(m => m.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.TreatmentHistory)
            .WithMany(t => t.Messages)
            .HasForeignKey(m => m.TreatmentHistoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.RequestId);
    }
}
